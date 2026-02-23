using System.Security.Cryptography;
using System.Text;

namespace VinCipher.Model;

/// <summary>
/// Generates and verifies HMAC-SHA256 challenge tokens for the playground.
/// The JS client must produce the same signature to prove it runs on our page.
/// Signature = HMAC-SHA256(secret, vin + "|" + timestamp) — timestamp in unix seconds.
/// A tolerance window prevents replay beyond a short time skew.
/// </summary>
public sealed class PlaygroundChallenge
{
    private readonly byte[] _secretKey;
    private readonly int _toleranceSeconds;

    public PlaygroundChallenge(string secret, int toleranceSeconds = 90)
    {
        ArgumentNullException.ThrowIfNull(secret);
        _secretKey = Encoding.UTF8.GetBytes(secret);
        _toleranceSeconds = toleranceSeconds;
    }

    /// <summary>
    /// Verifies the client-provided HMAC signature against the VIN and timestamp.
    /// </summary>
    public bool Verify(string vin, long timestamp, string clientSignature)
    {
        if (string.IsNullOrWhiteSpace(vin) || string.IsNullOrWhiteSpace(clientSignature))
            return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var drift = Math.Abs(now - timestamp);

        if (drift > _toleranceSeconds)
            return false;

        var expected = ComputeSignature(vin, timestamp);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(clientSignature));
    }

    private string ComputeSignature(string vin, long timestamp)
    {
        var payload = $"{vin}|{timestamp}";
        var hash = HMACSHA256.HashData(_secretKey, Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}
