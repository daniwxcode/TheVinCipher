using System.Collections.Concurrent;

namespace VinCipher.Model;

/// <summary>
/// In-memory rate limiter for the VIN decode endpoint.
/// Tracks requests per token with a rolling daily cap (default 50/day).
/// Resets at midnight UTC. Thread-safe via ConcurrentDictionary.
/// </summary>
public sealed class VinDecoderRateLimiter
{
    private readonly int _maxPerDay;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public VinDecoderRateLimiter(int maxPerDay)
    {
        _maxPerDay = maxPerDay;
    }

    /// <summary>
    /// Checks whether the given token is allowed to make a request.
    /// Returns remaining daily quota if allowed, or -1 remaining when blocked.
    /// </summary>
    public (bool Allowed, int RemainingDaily, int RetryAfterSeconds) TryAcquire(string token)
    {
        var now = DateTimeOffset.UtcNow;
        var bucket = _buckets.GetOrAdd(token, _ => new TokenBucket());

        lock (bucket)
        {
            bucket.ResetIfNewDay(now);

            if (bucket.DailyCount >= _maxPerDay)
            {
                var resetTime = bucket.DayStart.AddDays(1);
                var retryAfter = (int)Math.Ceiling((resetTime - now).TotalSeconds);
                return (false, 0, Math.Max(retryAfter, 1));
            }

            bucket.DailyCount++;
            return (true, _maxPerDay - bucket.DailyCount, 0);
        }
    }

    private sealed class TokenBucket
    {
        public int DailyCount { get; set; }
        public DateTimeOffset DayStart { get; set; } = DateTimeOffset.UtcNow.Date;

        public void ResetIfNewDay(DateTimeOffset now)
        {
            if (now.Date > DayStart.Date)
            {
                DailyCount = 0;
                DayStart = now.Date;
            }
        }
    }
}
