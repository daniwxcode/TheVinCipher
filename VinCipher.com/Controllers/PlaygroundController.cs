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
    private readonly VinDecodeCache _vinCache;
    private readonly int _previewFieldCount;
    private readonly ILogger<PlaygroundController> _logger;

    private static readonly HashSet<string> PreviewKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "make", "model", "model_year", "country", "body_style", "fuel_type"
    };

    public PlaygroundController(
        TokensProvider tokensProvider,
        VinRushScrapper vinRushScrapper,
        IConfiguration configuration,
        PlaygroundRateLimiter rateLimiter,
        PlaygroundChallenge challenge,
        VinDecoderRateLimiter decoderRateLimiter,
        PlaygroundDbContext pgDb,
        VinDecodeCache vinCache,
        IHttpClientFactory httpClientFactory,
        ILogger<PlaygroundController> logger)
    {
        _decoder = new VinDecoderController(tokensProvider, vinRushScrapper, decoderRateLimiter, vinCache, httpClientFactory);
        _playgroundToken = configuration["PlaygroundToken"]
            ?? throw new InvalidOperationException("PlaygroundToken is not configured.");
        _rateLimiter = rateLimiter;
        _challenge = challenge;
        _tokensProvider = tokensProvider;
        _pgDb = pgDb;
        _vinCache = vinCache;
        _previewFieldCount = configuration.GetValue("Playground:PreviewFieldCount", 6);
        _logger = logger;
    }

    /// <summary>
    /// Submits an access request for review by an admin.
    /// </summary>
    [HttpPost("request-access")]
    public async Task<ActionResult> RequestAccess(
        [FromBody] AccessRequestDto request,
        [FromQuery(Name = "t")] long timestamp,
        [FromQuery(Name = "s")] string signature)
    {
        if (!_challenge.Verify("request-access", timestamp, signature ?? ""))
            return StatusCode(403, new { error = "Signature invalide" });

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Nom et email requis." });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "Veuillez expliquer pourquoi vous souhaitez acc\u00e9der \u00e0 l\u2019API." });

        var existingAccount = await _pgDb.Accounts.AnyAsync(a => a.Email == request.Email);
        if (existingAccount)
            return Conflict(new { error = "Un compte avec cet email existe d\u00e9j\u00e0." });

        var pendingRequest = await _pgDb.AccessRequests
            .AnyAsync(r => r.Email == request.Email && r.Status == "pending");
        if (pendingRequest)
            return Conflict(new { error = "Une demande est d\u00e9j\u00e0 en cours de traitement pour cet email." });

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = $"{request.PhoneCode}{request.PhoneNumber}",
            PhoneCode = request.PhoneCode ?? "",
            Domain = request.Domain ?? "",
            Reason = request.Reason
        };

        _pgDb.AccessRequests.Add(accessRequest);
        await _pgDb.SaveChangesAsync();

        _logger.LogInformation("Access request submitted: {Email}", accessRequest.Email);

        return Ok(new
        {
            message = "Votre demande a \u00e9t\u00e9 enregistr\u00e9e. Un administrateur l\u2019examinera sous peu.",
            requestId = accessRequest.Id
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
                revokedAt = t.RevokedAtUtc,
                dailyLimit = t.DailyLimit
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

        var tokens = await _pgDb.ApiTokens
            .Where(t => t.AccountId == account.Id)
            .ToListAsync();

        var tokenIds = tokens.Select(t => t.Id).ToList();
        var logs = _pgDb.RequestLogs.Where(r => tokenIds.Contains(r.TokenId));

        var today = DateTime.UtcNow.Date;
        var totalRequests = await logs.CountAsync();
        var todayRequests = await logs.CountAsync(r => r.TimestampUtc >= today);
        var successCount = await logs.CountAsync(r => r.Success);
        var failureCount = totalRequests - successCount;
        var avgResponseTime = totalRequests > 0
            ? await logs.AverageAsync(r => (double)r.ResponseTimeMs)
            : 0;

        var activeTokens = tokens.Count(t => t.IsActive);
        var totalDailyLimit = tokens.Where(t => t.IsActive).Sum(t => t.DailyLimit);
        var remainingToday = totalDailyLimit > 0 ? Math.Max(0, totalDailyLimit - todayRequests) : -1;

        return Ok(new
        {
            totalRequests,
            todayRequests,
            dailyLimit = totalDailyLimit,
            remainingToday,
            successCount,
            failureCount,
            successRate = totalRequests > 0 ? Math.Round(100.0 * successCount / totalRequests, 1) : 0,
            avgResponseTimeMs = Math.Round(avgResponseTime),
            activeTokens
        });
    }

    /// <summary>
    /// Authenticated full decode — returns all fields, no preview truncation.
    /// Rate-limited by the token's DailyLimit.
    /// </summary>
    [HttpGet("full-decode")]
    public async Task<ActionResult<Dictionary<string, string>>> FullDecode([FromQuery] string vin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return BadRequest(new { error = "Le VIN est requis." });

        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Unauthorized(new { error = "Header X-Api-Key requis." });

        var token = await _pgDb.ApiTokens
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Key == apiKey && t.IsActive);

        if (token is null || token.ExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized(new { error = "Token invalide ou expiré." });

        // Per-token daily limit check
        var today = DateTime.UtcNow.Date;
        var todayCount = await _pgDb.RequestLogs
            .CountAsync(r => r.TokenId == token.Id && r.TimestampUtc >= today);

        var limit = token.DailyLimit;
        var remaining = limit > 0 ? Math.Max(0, limit - todayCount) : -1;

        if (limit > 0 && todayCount >= limit)
        {
            Response.Headers["X-RateLimit-Remaining"] = "0";
            return StatusCode(429, new
            {
                error = "Limite journalière atteinte",
                message = $"Vous avez utilisé {todayCount}/{limit} requêtes aujourd'hui.",
                dailyLimit = limit,
                todayCount,
                remaining = 0
            });
        }

        var startTime = Stopwatch.GetTimestamp();

        Dictionary<string, string>? data = null;
        if (_vinCache.TryGet(vin, out var cached))
            data = cached;

        var result = data is not null
            ? new ActionResult<Dictionary<string, string>>(Ok(data))
            : await _decoder.Decode(_playgroundToken, vin, cancellationToken);

        var elapsed = (int)Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

        // Log request
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
            ResponseTimeMs = elapsed
        });
        await _pgDb.SaveChangesAsync();

        if (result.Result is OkObjectResult ok && ok.Value is Dictionary<string, string> fullData)
        {
            if (data is null)
                _vinCache.Set(vin, fullData);

            var newRemaining = limit > 0 ? Math.Max(0, limit - todayCount - 1) : -1;
            Response.Headers["X-RateLimit-Remaining"] = newRemaining.ToString();

            fullData["_remaining"] = newRemaining.ToString();
            fullData["_dailyLimit"] = limit.ToString();
            fullData["_responseTimeMs"] = elapsed.ToString();
            return Ok(fullData);
        }

        return result;
    }

    /// <summary>
    /// Decodes a VIN after verifying the HMAC challenge signature.
    /// Logs the request result.
    /// </summary>
    [HttpGet("decode")]
    public async Task<ActionResult<Dictionary<string, string>>> Decode(
        [FromQuery] string vin,
        [FromQuery(Name = "t")] long timestamp,
        [FromQuery(Name = "s")] string signature,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return BadRequest("Le VIN est requis.");

        if (!_challenge.Verify(vin, timestamp, signature ?? ""))
        {
            _logger.LogWarning("Playground HMAC challenge failed for VIN {Vin}, t={Timestamp}", SanitizeForLog(vin), timestamp);
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

        // Check 24h in-memory cache first
        Dictionary<string, string>? cachedData = null;
        if (_vinCache.TryGet(vin, out var cached))
        {
            cachedData = cached;
        }

        var result = cachedData is not null
            ? new ActionResult<Dictionary<string, string>>(Ok(cachedData))
            : await _decoder.Decode(_playgroundToken, vin, cancellationToken);

        var elapsed = (int)Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

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

            // Cache the full decode result for 24h
            if (cachedData is null)
                _vinCache.Set(vin, fullData);

            preview["_remaining"] = remainingDaily.ToString();
            return Ok(preview);
        }

        return result;
    }

    private async Task LogRequestAsync(string vin, ActionResult<Dictionary<string, string>> result, int elapsedMs)
    {
        var statusCode = result.Result switch
        {
            OkObjectResult => 200,
            BadRequestObjectResult => 400,
            NotFoundObjectResult => 404,
            ObjectResult obj => obj.StatusCode ?? 500,
            _ => 200
        };

        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        PlaygroundApiToken? token = null;
        if (!string.IsNullOrEmpty(apiKey))
            token = await _pgDb.ApiTokens.FirstOrDefaultAsync(t => t.Key == apiKey && t.IsActive);

        // Always log — use token if authenticated, otherwise use the playground default token
        token ??= await _pgDb.ApiTokens.FirstOrDefaultAsync(t => t.Key == _playgroundToken && t.IsActive);
        if (token is null) return;

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

    private string GetClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            var ip = forwarded.Split(',', StringSplitOptions.TrimEntries)[0];
            return SanitizeForLog(ip);
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return SanitizeForLog(remoteIp);
    }

    /// <summary>
    /// Strips control characters (CR, LF) from a string to prevent log-forging.
    /// </summary>
    private static string SanitizeForLog(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();
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

public record AccessRequestDto(string Name, string Email, string? PhoneCode, string? PhoneNumber, string? Domain, string Reason);
public record LoginRequest(string Token);
