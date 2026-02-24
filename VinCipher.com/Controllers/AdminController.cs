using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VinCipher.Model;
using VinCipher.Model.Playground;
using VinCipher.Services;

namespace VinCipher.Controllers;

/// <summary>
/// Admin API for managing playground accounts, tokens, and admin users.
/// Authenticated via X-Admin-Key header (session key obtained from POST login).
/// Hidden from OpenAPI/Scalar documentation.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class AdminController : ControllerBase
{
    private readonly PlaygroundDbContext _db;
    private readonly TokensProvider _tokensProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminController> _logger;
    private readonly AdminEventBus _eventBus;

    public AdminController(
        PlaygroundDbContext db,
        TokensProvider tokensProvider,
        IConfiguration config,
        ILogger<AdminController> logger,
        AdminEventBus eventBus)
    {
        _db = db;
        _tokensProvider = tokensProvider;
        _config = config;
        _logger = logger;
        _eventBus = eventBus;
    }

    // ??? Authentication ????????????????????????????????????????????

    /// <summary>
    /// Admin login — returns a session key valid for N hours (config Admin:SessionDurationHours).
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username et password requis." });

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == request.Username);
        if (admin is null || !admin.VerifyPassword(request.Password))
        {
            _logger.LogWarning("Admin login failed for username {Username}", SanitizeForLog(request.Username));
            return Unauthorized(new { error = "Identifiants invalides." });
        }

        var hours = _config.GetValue("Admin:SessionDurationHours", 8);
        var plaintextSessionKey = $"ADM-{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}";
        admin.SessionKey = AdminUser.HashSessionKey(plaintextSessionKey);
        admin.SessionExpiresAtUtc = DateTime.UtcNow.AddHours(hours);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Username} logged in, session expires {Expires}", admin.Username, admin.SessionExpiresAtUtc);

        return Ok(new
        {
            sessionKey = plaintextSessionKey,
            expiresAt = admin.SessionExpiresAtUtc,
            username = admin.Username,
            isRoot = admin.IsRoot
        });
    }

    /// <summary>
    /// Admin logout — invalidates the session key.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        admin.SessionKey = null;
        admin.SessionExpiresAtUtc = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Déconnexion admin effectuée." });
    }

    // ??? Accounts ??????????????????????????????????????????????????

    /// <summary>
    /// Lists all playground accounts with token count and request stats.
    /// </summary>
    [HttpGet("accounts")]
    public async Task<ActionResult> ListAccounts([FromQuery] int page = 1, [FromQuery] int size = 25)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        size = Math.Clamp(size, 1, 100);
        page = Math.Max(page, 1);

        var total = await _db.Accounts.CountAsync();
        var accounts = await _db.Accounts
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new
            {
                id = a.Id,
                name = a.Name,
                email = a.Email,
                domain = a.Domain,
                createdAt = a.CreatedAtUtc,
                activeTokens = a.Tokens.Count(t => t.IsActive),
                totalTokens = a.Tokens.Count,
                totalRequests = a.Tokens.SelectMany(t => t.RequestLogs).Count()
            })
            .ToListAsync();

        return Ok(new { total, page, size, accounts });
    }

    /// <summary>
    /// Returns detailed info for a single account including all tokens and recent logs.
    /// </summary>
    [HttpGet("accounts/{accountId:guid}")]
    public async Task<ActionResult> GetAccount(Guid accountId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var account = await _db.Accounts
            .Include(a => a.Tokens)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account is null)
            return NotFound(new { error = "Compte non trouvé." });

        var tokenIds = account.Tokens.Select(t => t.Id).ToList();
        var recentLogs = await _db.RequestLogs
            .Where(r => tokenIds.Contains(r.TokenId))
            .OrderByDescending(r => r.TimestampUtc)
            .Take(50)
            .Select(r => new
            {
                vin = r.Vin,
                statusCode = r.StatusCode,
                success = r.Success,
                responseTimeMs = r.ResponseTimeMs,
                provider = r.Provider,
                timestamp = r.TimestampUtc,
                tokenId = r.TokenId
            })
            .ToListAsync();

        var totalRequests = await _db.RequestLogs.CountAsync(r => tokenIds.Contains(r.TokenId));

        var logsQuery = _db.RequestLogs.Where(r => tokenIds.Contains(r.TokenId));
        var today = DateTime.UtcNow.Date;
        var todayRequests = await logsQuery.CountAsync(r => r.TimestampUtc >= today);
        var successCount = await logsQuery.CountAsync(r => r.Success);
        var uniqueVins = await logsQuery.Select(r => r.Vin).Distinct().CountAsync();
        var avgResponseTime = totalRequests > 0
            ? await logsQuery.AverageAsync(r => (double)r.ResponseTimeMs)
            : 0;
        var topVins = await logsQuery
            .GroupBy(r => r.Vin)
            .Select(g => new { vin = g.Key, count = g.Count(), lastQueried = g.Max(r => r.TimestampUtc) })
            .OrderByDescending(g => g.count)
            .Take(10)
            .ToListAsync();

        var providerBreakdown = totalRequests > 0
            ? await logsQuery
                .GroupBy(r => r.Provider == "" ? "Inconnu" : r.Provider)
                .Select(g => new { provider = g.Key, count = g.Count() })
                .OrderByDescending(g => g.count)
                .ToListAsync()
            : [];

        return Ok(new
        {
            account = new
            {
                account.Id,
                account.Name,
                account.Email,
                account.Phone,
                account.PhoneCode,
                account.Domain,
                account.CreatedAtUtc
            },
            tokens = account.Tokens.OrderByDescending(t => t.CreatedAtUtc).Select(t => new
            {
                t.Id,
                t.Key,
                t.Name,
                t.IsActive,
                t.CreatedAtUtc,
                t.ExpiresAtUtc,
                t.RevokedAtUtc,
                t.DailyLimit
            }),
            totalRequests,
            vinStats = new
            {
                totalRequests,
                todayRequests,
                successCount,
                failureCount = totalRequests - successCount,
                successRate = totalRequests > 0 ? Math.Round(100.0 * successCount / totalRequests, 1) : 0,
                avgResponseTimeMs = Math.Round(avgResponseTime),
                uniqueVins,
                topVins,
                providerBreakdown = providerBreakdown.Select(p => new
                {
                    p.provider,
                    p.count,
                    percent = Math.Round(100.0 * p.count / totalRequests, 1)
                })
            },
            recentLogs
        });
    }

    /// <summary>
    /// Deletes a playground account and all its tokens/logs (cascade).
    /// </summary>
    [HttpDelete("accounts/{accountId:guid}")]
    public async Task<ActionResult> DeleteAccount(Guid accountId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var account = await _db.Accounts.FindAsync(accountId);
        if (account is null)
            return NotFound(new { error = "Compte non trouvé." });

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Admin} deleted account {AccountId} ({Email})", admin.Username, accountId, account.Email);
        return Ok(new { message = $"Compte {account.Email} supprimé." });
    }

    // ??? Token management ??????????????????????????????????????????

    /// <summary>
    /// Creates a new API token for any account (admin override — configurable duration).
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/tokens")]
    public async Task<ActionResult> CreateToken(Guid accountId, [FromBody] AdminCreateTokenRequest request)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var account = await _db.Accounts.FindAsync(accountId);
        if (account is null)
            return NotFound(new { error = "Compte non trouvé." });

        var durationDays = request.DurationDays > 0 ? request.DurationDays : 1;
        var token = new PlaygroundApiToken
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Key = $"PG-{Convert.ToHexString(RandomNumberGenerator.GetBytes(20))}",
            Name = request.Name ?? $"Admin-created",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(durationDays)
        };

        _db.ApiTokens.Add(token);
        await _db.SaveChangesAsync();

        _tokensProvider.AddPlaygroundToken(token.Key, token.ExpiresAtUtc);

        _logger.LogInformation("Admin {Admin} created token for account {AccountId}, expires {Expires}", admin.Username, accountId, token.ExpiresAtUtc);

        return Ok(new
        {
            id = token.Id,
            key = token.Key,
            name = token.Name,
            expiresAt = token.ExpiresAtUtc,
            dailyLimit = token.DailyLimit
        });
    }

    /// <summary>
    /// Revokes a token for any account.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/tokens/{tokenId:guid}/revoke")]
    public async Task<ActionResult> RevokeToken(Guid accountId, Guid tokenId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var token = await _db.ApiTokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.AccountId == accountId);
        if (token is null)
            return NotFound(new { error = "Token non trouvé." });

        token.IsActive = false;
        token.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Admin} revoked token {TokenId} for account {AccountId}", admin.Username, tokenId, accountId);
        return Ok(new { message = "Token révoqué." });
    }

    /// <summary>
    /// Extends the expiry of an existing token.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/tokens/{tokenId:guid}/extend")]
    public async Task<ActionResult> ExtendToken(Guid accountId, Guid tokenId, [FromBody] AdminExtendTokenRequest request)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var token = await _db.ApiTokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.AccountId == accountId);
        if (token is null)
            return NotFound(new { error = "Token non trouvé." });

        var days = request.AdditionalDays > 0 ? request.AdditionalDays : 1;
        var baseline = token.ExpiresAtUtc > DateTime.UtcNow ? token.ExpiresAtUtc : DateTime.UtcNow;
        token.ExpiresAtUtc = baseline.AddDays(days);
        token.IsActive = true;
        token.RevokedAtUtc = null;
        await _db.SaveChangesAsync();

        _tokensProvider.AddPlaygroundToken(token.Key, token.ExpiresAtUtc);

        _logger.LogInformation("Admin {Admin} extended token {TokenId} by {Days}d, new expiry {Expires}", admin.Username, tokenId, days, token.ExpiresAtUtc);
        return Ok(new { message = "Token étendu.", newExpiresAt = token.ExpiresAtUtc });
    }

    /// <summary>
    /// Updates the daily request limit for a token. 0 = unlimited.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/tokens/{tokenId:guid}/daily-limit")]
    public async Task<ActionResult> SetDailyLimit(Guid accountId, Guid tokenId, [FromBody] AdminSetDailyLimitRequest request)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var token = await _db.ApiTokens.FirstOrDefaultAsync(t => t.Id == tokenId && t.AccountId == accountId);
        if (token is null)
            return NotFound(new { error = "Token non trouvé." });

        token.DailyLimit = Math.Max(0, request.DailyLimit);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Admin} set daily limit for token {TokenId} to {Limit}", admin.Username, tokenId, token.DailyLimit);
        return Ok(new { message = $"Limite fixée à {token.DailyLimit} requêtes/jour.", dailyLimit = token.DailyLimit });
    }

    // ??? Admin user management ??????????????????????????????????????

    /// <summary>
    /// Lists all admin users. Requires any admin session.
    /// </summary>
    [HttpGet("admins")]
    public async Task<ActionResult> ListAdmins()
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var admins = await _db.AdminUsers
            .OrderBy(a => a.CreatedAtUtc)
            .Select(a => new
            {
                a.Id,
                a.Username,
                a.IsRoot,
                a.CreatedAtUtc,
                hasActiveSession = a.SessionExpiresAtUtc != null && a.SessionExpiresAtUtc > DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(new { admins });
    }

    /// <summary>
    /// Creates a new admin user. Only root admins can do this.
    /// </summary>
    [HttpPost("admins")]
    public async Task<ActionResult> CreateAdmin([FromBody] AdminCreateRequest request)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        if (!admin.IsRoot)
            return StatusCode(403, new { error = "Seul le compte root peut créer des administrateurs." });

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Username et password requis." });

        if (request.Password.Length < 8)
            return BadRequest(new { error = "Le mot de passe doit contenir au moins 8 caractères." });

        if (await _db.AdminUsers.AnyAsync(a => a.Username == request.Username))
            return Conflict(new { error = "Ce username existe déjà." });

        var newAdmin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            IsRoot = false
        };
        newAdmin.SetPassword(request.Password);

        _db.AdminUsers.Add(newAdmin);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Root admin {Admin} created admin user {NewAdmin}", admin.Username, request.Username);

        return Ok(new
        {
            id = newAdmin.Id,
            username = newAdmin.Username,
            isRoot = newAdmin.IsRoot,
            createdAt = newAdmin.CreatedAtUtc
        });
    }

    /// <summary>
    /// Deletes an admin user. Only root can delete. Cannot delete self.
    /// </summary>
    [HttpDelete("admins/{adminId:guid}")]
    public async Task<ActionResult> DeleteAdmin(Guid adminId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        if (!admin.IsRoot)
            return StatusCode(403, new { error = "Seul le compte root peut supprimer des administrateurs." });

        if (admin.Id == adminId)
            return BadRequest(new { error = "Impossible de supprimer votre propre compte." });

        var target = await _db.AdminUsers.FindAsync(adminId);
        if (target is null)
            return NotFound(new { error = "Admin non trouvé." });

        _db.AdminUsers.Remove(target);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Root admin {Admin} deleted admin user {Target}", admin.Username, target.Username);
        return Ok(new { message = $"Admin {target.Username} supprimé." });
    }

    // ??? Global stats ??????????????????????????????????????????????

    /// <summary>
    /// Returns platform-wide stats.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GlobalStats()
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var today = DateTime.UtcNow.Date;
        var totalRequests = await _db.RequestLogs.CountAsync();

        var providerBreakdown = totalRequests > 0
            ? await _db.RequestLogs
                .GroupBy(r => r.Provider == "" ? "Inconnu" : r.Provider)
                .Select(g => new { provider = g.Key, count = g.Count() })
                .OrderByDescending(g => g.count)
                .ToListAsync()
            : [];

        return Ok(new
        {
            totalAccounts = await _db.Accounts.CountAsync(),
            totalTokens = await _db.ApiTokens.CountAsync(),
            activeTokens = await _db.ApiTokens.CountAsync(t => t.IsActive && t.ExpiresAtUtc > DateTime.UtcNow),
            totalRequests,
            todayRequests = await _db.RequestLogs.CountAsync(r => r.TimestampUtc >= today),
            totalAdmins = await _db.AdminUsers.CountAsync(),
            pendingRequests = await _db.AccessRequests.CountAsync(r => r.Status == "pending"),
            providerBreakdown = providerBreakdown.Select(p => new
            {
                p.provider,
                p.count,
                percent = Math.Round(100.0 * p.count / totalRequests, 1)
            })
        });
    }

    // ??? Access requests management ?????????????????????????????????

    /// <summary>
    /// Lists all access requests, optionally filtered by status.
    /// </summary>
    [HttpGet("access-requests")]
    public async Task<ActionResult> ListAccessRequests(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 25)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        size = Math.Clamp(size, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.AccessRequests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        var total = await query.CountAsync();
        var requests = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Email,
                r.Phone,
                r.Domain,
                r.Reason,
                r.Status,
                r.CreatedAtUtc,
                r.ReviewedAtUtc,
                r.ReviewedBy,
                r.AccountId
            })
            .ToListAsync();

        return Ok(new { total, page, size, requests });
    }

    /// <summary>
    /// Returns full details for a single access request.
    /// </summary>
    [HttpGet("access-requests/{requestId:guid}")]
    public async Task<ActionResult> GetAccessRequest(Guid requestId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var request = await _db.AccessRequests.FindAsync(requestId);
        if (request is null)
            return NotFound(new { error = "Demande non trouv\u00e9e." });

        return Ok(new
        {
            request.Id,
            request.Name,
            request.Email,
            request.Phone,
            request.PhoneCode,
            request.Domain,
            request.Reason,
            request.Status,
            request.CreatedAtUtc,
            request.ReviewedAtUtc,
            request.ReviewedBy,
            request.AccountId
        });
    }

    /// <summary>
    /// Approves an access request: creates an account and a first API token.
    /// </summary>
    [HttpPost("access-requests/{requestId:guid}/approve")]
    public async Task<ActionResult> ApproveAccessRequest(Guid requestId, [FromBody] AdminApproveRequestDto? dto)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var request = await _db.AccessRequests.FindAsync(requestId);
        if (request is null)
            return NotFound(new { error = "Demande non trouv\u00e9e." });

        if (request.Status != "pending")
            return BadRequest(new { error = $"Cette demande est d\u00e9j\u00e0 {request.Status}." });

        var account = new PlaygroundAccount
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PhoneCode = request.PhoneCode,
            Domain = request.Domain
        };

        var durationDays = dto?.DurationDays > 0 ? dto.DurationDays : 30;
        var apiToken = new PlaygroundApiToken
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Key = $"PG-{Convert.ToHexString(RandomNumberGenerator.GetBytes(20))}",
            Name = "Cl\u00e9 initiale",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(durationDays)
        };

        request.Status = "approved";
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewedBy = admin.Username;
        request.AccountId = account.Id;

        _db.Accounts.Add(account);
        _db.ApiTokens.Add(apiToken);
        await _db.SaveChangesAsync();

        _tokensProvider.AddPlaygroundToken(apiToken.Key, apiToken.ExpiresAtUtc);

        _logger.LogInformation("Admin {Admin} approved access request {RequestId} for {Email}, account {AccountId}",
            admin.Username, requestId, request.Email, account.Id);

        return Ok(new
        {
            message = $"Demande approuv\u00e9e. Compte cr\u00e9\u00e9 pour {request.Email}.",
            accountId = account.Id,
            tokenKey = apiToken.Key,
            expiresAt = apiToken.ExpiresAtUtc
        });
    }

    /// <summary>
    /// Rejects an access request.
    /// </summary>
    [HttpPost("access-requests/{requestId:guid}/reject")]
    public async Task<ActionResult> RejectAccessRequest(Guid requestId)
    {
        var (admin, err) = await AuthenticateAdminAsync();
        if (admin is null) return err!;

        var request = await _db.AccessRequests.FindAsync(requestId);
        if (request is null)
            return NotFound(new { error = "Demande non trouv\u00e9e." });

        if (request.Status != "pending")
            return BadRequest(new { error = $"Cette demande est d\u00e9j\u00e0 {request.Status}." });

        request.Status = "rejected";
        request.ReviewedAtUtc = DateTime.UtcNow;
        request.ReviewedBy = admin.Username;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Admin} rejected access request {RequestId} for {Email}",
            admin.Username, requestId, request.Email);

        return Ok(new { message = $"Demande de {request.Email} refus\u00e9e." });
    }

    // ??? Real-time SSE stream ??????????????????????????????????????

    /// <summary>
    /// Server-Sent Events stream for real-time admin dashboard updates.
    /// Accepts session key via query param since EventSource cannot send headers.
    /// </summary>
    [HttpGet("events")]
    public async Task Events([FromQuery] string key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Response.StatusCode = 401;
            return;
        }

        var hashedKey = AdminUser.HashSessionKey(key);
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.SessionKey == hashedKey);
        if (admin is null || admin.SessionExpiresAtUtc is null || admin.SessionExpiresAtUtc < DateTime.UtcNow)
        {
            Response.StatusCode = 401;
            return;
        }

        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var reader = _eventBus.Subscribe();
        try
        {
            await foreach (var evt in reader.ReadAllAsync(cancellationToken))
            {
                await Response.WriteAsync(evt.ToSseData(), cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _eventBus.Unsubscribe(reader);
        }
    }

    // ??? Failed VINs analytics ???????????????????????????????????????

    /// <summary>
    /// Returns VINs that consistently fail decoding, grouped by VIN,
    /// with providers attempted and failure count.
    /// </summary>
    [HttpGet("failed-vins")]
    public async Task<ActionResult> FailedVins(
        [FromQuery] int page = 1,
        [FromQuery] int size = 30,
        [FromQuery] int minFailures = 1)
    {
        var (admin, errResult) = await AuthenticateAdminAsync();
        if (admin is null) return errResult!;

        size = Math.Clamp(size, 1, 100);
        page = Math.Max(page, 1);
        minFailures = Math.Max(1, minFailures);

        // Simple projection SQLite can handle — group in memory
        var failedLogs = await _db.RequestLogs
            .Where(r => !r.Success)
            .Select(r => new { r.Vin, r.StatusCode, r.Provider, r.TimestampUtc })
            .ToListAsync();

        var succeededVins = await _db.RequestLogs
            .Where(r => r.Success)
            .Select(r => r.Vin)
            .Distinct()
            .ToListAsync();

        var succeededSet = succeededVins.ToHashSet();

        var failedGroups = failedLogs
            .GroupBy(r => r.Vin)
            .Where(g => g.Count() >= minFailures)
            .Select(g => new
            {
                vin = g.Key,
                failureCount = g.Count(),
                lastAttempt = g.Max(r => r.TimestampUtc),
                statusCodes = g.Select(r => r.StatusCode).Distinct().OrderBy(c => c).ToList(),
                providersTried = g.Select(r => r.Provider).Where(p => p != "").Distinct().ToList(),
                hasSucceeded = succeededSet.Contains(g.Key)
            })
            .OrderByDescending(g => g.failureCount)
            .ToList();

        var total = failedGroups.Count;
        var paged = failedGroups
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        var neverSucceeded = paged.Count(v => !v.hasSucceeded);

        return Ok(new
        {
            total,
            page,
            size,
            neverSucceeded,
            vins = paged
        });
    }

    // ??? Helpers ????????????????????????????????????????????????????

    private async Task<(AdminUser? Admin, ActionResult? Error)> AuthenticateAdminAsync()
    {
        var key = Request.Headers["X-Admin-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(key))
            return (null, Unauthorized(new { error = "Header X-Admin-Key requis." }));

        var hashedKey = AdminUser.HashSessionKey(key);
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.SessionKey == hashedKey);
        if (admin is null || admin.SessionExpiresAtUtc is null || admin.SessionExpiresAtUtc < DateTime.UtcNow)
            return (null, Unauthorized(new { error = "Session admin invalide ou expirée." }));

        return (admin, null);
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
}

public record AdminLoginRequest(string Username, string Password);
public record AdminCreateRequest(string Username, string Password);
public record AdminCreateTokenRequest(string? Name, int DurationDays);
public record AdminExtendTokenRequest(int AdditionalDays);
public record AdminSetDailyLimitRequest(int DailyLimit);
public record AdminApproveRequestDto(int DurationDays);
