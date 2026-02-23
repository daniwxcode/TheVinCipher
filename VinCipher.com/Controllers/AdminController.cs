using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VinCipher.Model;
using VinCipher.Model.Playground;

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

    public AdminController(
        PlaygroundDbContext db,
        TokensProvider tokensProvider,
        IConfiguration config,
        ILogger<AdminController> logger)
    {
        _db = db;
        _tokensProvider = tokensProvider;
        _config = config;
        _logger = logger;
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
            _logger.LogWarning("Admin login failed for username {Username}", request.Username);
            return Unauthorized(new { error = "Identifiants invalides." });
        }

        var hours = _config.GetValue("Admin:SessionDurationHours", 8);
        admin.SessionKey = $"ADM-{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}";
        admin.SessionExpiresAtUtc = DateTime.UtcNow.AddHours(hours);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {Username} logged in, session expires {Expires}", admin.Username, admin.SessionExpiresAtUtc);

        return Ok(new
        {
            sessionKey = admin.SessionKey,
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
                timestamp = r.TimestampUtc,
                tokenId = r.TokenId
            })
            .ToListAsync();

        var totalRequests = await _db.RequestLogs.CountAsync(r => tokenIds.Contains(r.TokenId));

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
                t.RevokedAtUtc
            }),
            totalRequests,
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
            expiresAt = token.ExpiresAtUtc
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

    // ??? Admin user management ?????????????????????????????????????

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

        return Ok(new
        {
            totalAccounts = await _db.Accounts.CountAsync(),
            totalTokens = await _db.ApiTokens.CountAsync(),
            activeTokens = await _db.ApiTokens.CountAsync(t => t.IsActive && t.ExpiresAtUtc > DateTime.UtcNow),
            totalRequests = await _db.RequestLogs.CountAsync(),
            todayRequests = await _db.RequestLogs.CountAsync(r => r.TimestampUtc >= today),
            totalAdmins = await _db.AdminUsers.CountAsync()
        });
    }

    // ??? Helpers ????????????????????????????????????????????????????

    private async Task<(AdminUser? Admin, ActionResult? Error)> AuthenticateAdminAsync()
    {
        var key = Request.Headers["X-Admin-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(key))
            return (null, Unauthorized(new { error = "Header X-Admin-Key requis." }));

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.SessionKey == key);
        if (admin is null || admin.SessionExpiresAtUtc is null || admin.SessionExpiresAtUtc < DateTime.UtcNow)
            return (null, Unauthorized(new { error = "Session admin invalide ou expirée." }));

        return (admin, null);
    }
}

public record AdminLoginRequest(string Username, string Password);
public record AdminCreateRequest(string Username, string Password);
public record AdminCreateTokenRequest(string? Name, int DurationDays);
public record AdminExtendTokenRequest(int AdditionalDays);
