using Microsoft.Extensions.Caching.Memory;

namespace VinCipher.Model;

/// <summary>
/// In-memory cache for successfully decoded VINs.
/// Entries expire after 24 hours (absolute expiration).
/// </summary>
public sealed class VinDecodeCache
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions
    {
        SizeLimit = 10_000
    });

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30),
        Size = 1
    };

    /// <summary>
    /// Tries to get a cached decode result for the given VIN.
    /// </summary>
    public bool TryGet(string vin, out Dictionary<string, string> result)
    {
        return _cache.TryGetValue(vin.ToUpperInvariant(), out result!);
    }

    /// <summary>
    /// Stores a successful decode result in cache.
    /// </summary>
    public void Set(string vin, Dictionary<string, string> result)
    {
        _cache.Set(vin.ToUpperInvariant(), result, CacheOptions);
    }
}
