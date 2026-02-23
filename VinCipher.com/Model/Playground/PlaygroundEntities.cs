using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace VinCipher.Model.Playground;

public class PlaygroundAccount
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [Required, MaxLength(200)]
    public string Email { get; set; } = "";

    [MaxLength(30)]
    public string Phone { get; set; } = "";

    [MaxLength(10)]
    public string PhoneCode { get; set; } = "";

    [MaxLength(60)]
    public string Domain { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PlaygroundApiToken> Tokens { get; set; } = [];
}

public class PlaygroundApiToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [Required, MaxLength(100)]
    public string Key { get; set; } = "";

    [MaxLength(60)]
    public string Name { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public PlaygroundAccount Account { get; set; } = null!;

    public ICollection<RequestLog> RequestLogs { get; set; } = [];
}

public class RequestLog
{
    [Key]
    public long Id { get; set; }

    public Guid TokenId { get; set; }

    [MaxLength(17)]
    public string Vin { get; set; } = "";

    public int StatusCode { get; set; }

    public bool Success { get; set; }

    public int ResponseTimeMs { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public PlaygroundApiToken Token { get; set; } = null!;
}

public class AdminUser
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(80)]
    public string Username { get; set; } = "";

    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = "";

    public bool IsRoot { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session key generated on login, used to authenticate admin API calls.
    /// Rotated on every login.
    /// </summary>
    [MaxLength(100)]
    public string? SessionKey { get; set; }

    public DateTime? SessionExpiresAtUtc { get; set; }

    public void SetPassword(string plaintext)
    {
        using var derive = new Rfc2898DeriveBytes(plaintext, 16, 100_000, HashAlgorithmName.SHA256);
        var salt = derive.Salt;
        var hash = derive.GetBytes(32);
        PasswordHash = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string plaintext)
    {
        var parts = PasswordHash.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);
        using var derive = new Rfc2898DeriveBytes(plaintext, salt, 100_000, HashAlgorithmName.SHA256);
        var computedHash = derive.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
