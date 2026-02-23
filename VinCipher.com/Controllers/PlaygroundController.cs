using System.Diagnostics;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Services.DataServices;
using Services.Interfaces;

using VinCipher.Model;
using VinCipher.Model.Playground;

namespace VinCipher.Controllers;

/// <summary>
/// Proxy endpoint for the landing page playground.
/// Secured by: HMAC challenge (VIN+timestamp signed by shared secret),
/// IP rate limiting, response truncation.
/// Hidden from OpenAPI/Scalar documentation.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class PlaygroundController : ControllerBase
{
    private readonly VinDecoderController _decoder;
    private readonly string _playgroundToken;
    private readonly PlaygroundRateLimiter _rateLimiter;
    private readonly PlaygroundChallenge _challenge;
    private readonly TokensProvider _tokensProvider;
    private readonly PlaygroundDbContext _pgDb;
    private readonly int _previewFieldCount;
    private readonly ILogger<PlaygroundController> _logger;

    private static readonly HashSet<string> PreviewKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "make", "model", "model_year", "country", "body_style", "fuel_type"
    };

    public PlaygroundController(
        TokensProvider tokensProvider,
        VinRushScrapper vinRushScrapper,
        Infrastructure.Contexts.VinCipherContext context,
        ICrudServices crudServices,
        IConfiguration configuration,
        PlaygroundRateLimiter rateLimiter,
        PlaygroundChallenge challenge,
        VinDecoderRateLimiter decoderRateLimiter,
        PlaygroundDbContext pgDb,
        ILogger<PlaygroundController> logger)
    {
        _decoder = new VinDecoderController(tokensProvider, vinRushScrapper, context, crudServices, decoderRateLimiter);
        _playgroundToken = configuration["PlaygroundToken"]
            ?? throw new InvalidOperationException("PlaygroundToken is not configured.");
        _rateLimiter = rateLimiter;
        _challenge = challenge;
        _tokensProvider = tokensProvider;
        _pgDb = pgDb;
        _previewFieldCount = configuration.GetValue("Playground:PreviewFieldCount", 6);
        _logger = logger;
    }

    /// <summary>
    /// Creates a playground account with a first API token (valid 1 day / 50 requests).
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromQuery(Name = "t")] long timestamp,
        [FromQuery(Name = "s")] string signature)
    {
        if (!_challenge.Verify("register", timestamp, signature ?? ""))
            return StatusCode(403, new { error = "Signature invalide" });

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Nom et email requis." });

        var existing = await _pgDb.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
        if (existing is not null)
        {
            var existingToken = await _pgDb.ApiTokens
                .Where(t => t.AccountId == existing.Id && t.IsActive)
                .OrderByDescending(t => t.CreatedAtUtc)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                accountId = existing.Id,
                name = existing.Name,
                token = existingToken?.Key,
                expiresAt = existingToken?.ExpiresAtUtc,
                message = "Compte existant retrouvé."
            });
        }

        var account = new PlaygroundAccount
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = $"{request.PhoneCode}{request.PhoneNumber}",
            PhoneCode = request.PhoneCode ?? "",
            Domain = request.Domain ?? ""
        };

        var apiToken = CreateApiToken(account.Id, "Clé par défaut");

        _pgDb.Accounts.Add(account);
        _pgDb.ApiTokens.Add(apiToken);
        await _pgDb.SaveChangesAsync();

        _tokensProvider.AddPlaygroundToken(apiToken.Key, apiToken.ExpiresAtUtc);

        _logger.LogInformation("Playground account created: {Email}", account.Email);

        return Ok(new
        {
            accountId = account.Id,
            name = account.Name,
            token = apiToken.Key,
            expiresAt = apiToken.ExpiresAtUtc,
            maxRequests = 50
        });
    }

    /// <summary>
    /// Authenticates by API token key and returns account info.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "Token requis." });

        var apiToken = await _pgDb.ApiTokens
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Key == request.Token && t.IsActive);

        if (apiToken is null || apiToken.ExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized(new { error = "Token invalide ou expiré." });

        return Ok(new
        {
            accountId = apiToken.Account.Id,
            name = apiToken.Account.Name,
            email = apiToken.Account.Email,
            phone = apiToken.Account.Phone,
            domain = apiToken.Account.Domain,
            createdAt = apiToken.Account.CreatedAtUtc
        });
    }

    /// <summary>
    /// Returns account info and tokens for the authenticated user (X-Api-Key header).
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult> Me()
    {
        var (account, error) = await AuthenticateAsync();
        if (account is null) return error!;

        var tokens = await _pgDb.ApiTokens
            .Where(t => t.AccountId == account.Id)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new
            {
                id = t.Id,
                key = t.IsActive ? t.Key : new string('•', 24),
                name = t.Name,
                active = t.IsActive,
                createdAt = t.CreatedAtUtc,
                expiresAt = t.ExpiresAtUtc,
                revokedAt = t.RevokedAtUtc
            })
            .ToListAsync();

        return Ok(new
        {
            accountId = account.Id,
            name = account.Name,
            email = account.Email,
            phone = account.Phone,
            domain = account.Domain,
            createdAt = account.CreatedAtUtc,
            tokens
        });
    }

    /// <summary>
    /// Returns request history for the authenticated user.
    /// </summary>
    [HttpGet("me/history")]
    public async Task<ActionResult> History([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var (account, error) = await AuthenticateAsync();
        if (account is null) return error!;

        size = Math.Clamp(size, 1, 100);
        page = Math.Max(page, 1);

        var tokenIds = await _pgDb.ApiTokens
            .Where(t => t.AccountId == account.Id)
            .Select(t => t.Id)
            .ToListAsync();

        var query = _pgDb.RequestLogs
            .Where(r => tokenIds.Contains(r.TokenId))
            .OrderByDescending(r => r.TimestampUtc);

        var total = await query.CountAsync();
        var logs = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(r => new
            {
                vin = r.Vin,
                statusCode = r.StatusCode,
                success = r.Success,
                responseTimeMs = r.ResponseTimeMs,
                timestamp = r.TimestampUtc
            })
            .ToListAsync();

        return Ok(new { total, page, size, logs });
    }

    /// <summary>
    /// Returns aggregated stats for the authenticated user.
    /// </summary>
    [HttpGet("me/stats")]
    public async Task<ActionResult> Stats()
    {
        var (account, error) = await AuthenticateAsync();
        if (account is null) return error!;

        var tokenIds = await _pgDb.ApiTokens
            .Where(t => t.AccountId == account.Id)
            .Select(t => t.Id)
            .ToListAsync();

        var logs = _pgDb.RequestLogs.Where(r => tokenIds.Contains(r.TokenId));

        var today = DateTime.UtcNow.Date;
        var totalRequests = await logs.CountAsync();
        var todayRequests = await logs.CountAsync(r => r.TimestampUtc >= today);
        var successCount = await logs.CountAsync(r => r.Success);
        var failureCount = totalRequests - successCount;
        var avgResponseTime = totalRequests > 0
            ? await logs.AverageAsync(r => (double)r.ResponseTimeMs)
            : 0;

        var activeTokens = await _pgDb.ApiTokens.CountAsync(t => t.AccountId == account.Id && t.IsActive);

        return Ok(new
        {
            totalRequests,
            todayRequests,
            remainingToday = Math.Max(0, 50 - todayRequests),
            successCount,
            failureCount,
            successRate = totalRequests > 0 ? Math.Round(100.0 * successCount / totalRequests, 1) : 0,
            avgResponseTimeMs = Math.Round(avgResponseTime),
            activeTokens
        });
    }

    /// <summary>
    /// Generates a new API token for the authenticated account (valid 1 day / 50 requests).
    /// </summary>
    [HttpPost("generate-token")]
    public async Task<ActionResult> GenerateToken()
    {
        var (account, error) = await AuthenticateAsync();
        if (account is null) return error!;

        var activeCount = await _pgDb.ApiTokens.CountAsync(t => t.AccountId == account.Id && t.IsActive);
        if (activeCount >= 5)
            return BadRequest(new { error = "Maximum 5 tokens actifs par compte." });

        var apiToken = CreateApiToken(account.Id, $"Clé #{activeCount + 1}");
        _pgDb.ApiTokens.Add(apiToken);
        await _pgDb.SaveChangesAsync();

        _tokensProvider.AddPlaygroundToken(apiToken.Key, apiToken.ExpiresAtUtc);

        return Ok(new
        {
            id = apiToken.Id,
            token = apiToken.Key,
            name = apiToken.Name,
            expiresAt = apiToken.ExpiresAtUtc,
            maxRequests = 50
        });
    }

    /// <summary>
    /// Revokes a token by ID.
    /// </summary>
    [HttpPost("revoke-token/{tokenId:guid}")]
    public async Task<ActionResult> RevokeToken(Guid tokenId)
    {
        var (account, error) = await AuthenticateAsync();
        if (account is null) return error!;

        var token = await _pgDb.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.AccountId == account.Id && t.IsActive);

        if (token is null)
            return NotFound(new { error = "Token non trouvé." });

        token.IsActive = false;
        token.RevokedAtUtc = DateTime.UtcNow;
        await _pgDb.SaveChangesAsync();

        return Ok(new { message = "Token révoqué." });
    }

    /// <summary>
    /// Decodes a VIN after verifying the HMAC challenge signature.
    /// Logs the request result.
    /// </summary>
    [HttpGet("decode")]
    public async Task<ActionResult<Dictionary<string, string>>> Decode(
        [FromQuery] string vin,
        [FromQuery(Name = "t")] long timestamp,
        [FromQuery(Name = "s")] string signature)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return BadRequest("Le VIN est requis.");

        if (!_challenge.Verify(vin, timestamp, signature ?? ""))
        {
            _logger.LogWarning("Playground HMAC challenge failed for VIN {Vin}, t={Timestamp}", vin, timestamp);
            return StatusCode(403, new { error = "Signature invalide", message = "Requête non autorisée." });
        }

        var ip = GetClientIp();
        var (allowed, remainingDaily, retryAfter) = _rateLimiter.TryAcquire(ip);

        Response.Headers["X-RateLimit-Remaining"] = remainingDaily.ToString();

        if (!allowed)
        {
            Response.Headers["Retry-After"] = retryAfter.ToString();
            _logger.LogWarning("Playground rate limit exceeded for IP {Ip}", ip);
            return StatusCode(429, new
            {
                error = "Limite atteinte",
                message = $"Vous avez atteint la limite de décodages gratuits. Réessayez dans {FormatRetry(retryAfter)}.",
                retryAfterSeconds = retryAfter
            });
        }

        var startTime = Stopwatch.GetTimestamp();

        var result = await _decoder.Decode(_playgroundToken, vin);

        var elapsed = (int)Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

        // Log request if caller has X-Api-Key
        await LogRequestAsync(vin, result, elapsed);

        if (result.Result is OkObjectResult ok && ok.Value is Dictionary<string, string> fullData)
        {
            var totalFields = fullData.Count;
            var preview = new Dictionary<string, string>();
            var added = 0;

            foreach (var key in fullData.Keys)
            {
                if (added >= _previewFieldCount)
                    break;

                if (PreviewKeys.Contains(key) || added < _previewFieldCount)
                {
                    preview[key] = fullData[key];
                    added++;
                }
            }

            var hiddenCount = totalFields - preview.Count;
            if (hiddenCount > 0)
                preview["_demo"] = $"Aperçu limité — {hiddenCount} champs masqués. Souscrivez pour l\u2019accès complet.";

            preview["_remaining"] = remainingDaily.ToString();
            return Ok(preview);
        }

        return result;
    }

    private async Task LogRequestAsync(string vin, ActionResult<Dictionary<string, string>> result, int elapsedMs)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey)) return;

        var token = await _pgDb.ApiTokens.FirstOrDefaultAsync(t => t.Key == apiKey && t.IsActive);
        if (token is null) return;

        var statusCode = result.Result switch
        {
            OkObjectResult => 200,
            BadRequestObjectResult => 400,
            NotFoundObjectResult => 404,
            ObjectResult obj => obj.StatusCode ?? 500,
            _ => 200
        };

        _pgDb.RequestLogs.Add(new RequestLog
        {
            TokenId = token.Id,
            Vin = vin,
            StatusCode = statusCode,
            Success = statusCode is >= 200 and < 300,
            ResponseTimeMs = elapsedMs
        });

        await _pgDb.SaveChangesAsync();
    }

    private async Task<(PlaygroundAccount? Account, ActionResult? Error)> AuthenticateAsync()
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return (null, Unauthorized(new { error = "Header X-Api-Key requis." }));

        var token = await _pgDb.ApiTokens
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Key == apiKey && t.IsActive);

        if (token is null || token.ExpiresAtUtc < DateTime.UtcNow)
            return (null, Unauthorized(new { error = "Token invalide ou expiré." }));

        return (token.Account, null);
    }

    private static PlaygroundApiToken CreateApiToken(Guid accountId, string name)
    {
        return new PlaygroundApiToken
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Key = $"PG-{Convert.ToHexString(RandomNumberGenerator.GetBytes(20))}",
            Name = name,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };
    }

    private string GetClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',', StringSplitOptions.TrimEntries)[0];

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string FormatRetry(int seconds)
    {
        if (seconds >= 3600)
            return $"{seconds / 3600}h{(seconds % 3600) / 60:D2}";
        if (seconds >= 60)
            return $"{seconds / 60} min";
        return $"{seconds}s";
    }
}

public record RegisterRequest(string Name, string Email, string? PhoneCode, string? PhoneNumber, string? Domain);
public record LoginRequest(string Token);
