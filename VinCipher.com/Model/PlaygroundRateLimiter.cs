using System.Collections.Concurrent;

namespace VinCipher.Model;

/// <summary>
/// In-memory rate limiter for the playground endpoint.
/// Tracks requests per IP with a sliding minute window and a rolling daily cap.
/// Thread-safe via ConcurrentDictionary.
/// </summary>
public sealed class PlaygroundRateLimiter
{
    private readonly int _maxPerMinute;
    private readonly int _maxPerDay;
    private readonly ConcurrentDictionary<string, IpBucket> _buckets = new();

    public PlaygroundRateLimiter(int maxPerMinute, int maxPerDay)
    {
        _maxPerMinute = maxPerMinute;
        _maxPerDay = maxPerDay;
    }

    /// <summary>
    /// Checks whether the given IP is allowed to make a request.
    /// Returns remaining daily quota if allowed, or -1 if blocked.
    /// </summary>
    public (bool Allowed, int RemainingDaily, int RetryAfterSeconds) TryAcquire(string ip)
    {
        var now = DateTimeOffset.UtcNow;
        var bucket = _buckets.GetOrAdd(ip, _ => new IpBucket());

        lock (bucket)
        {
            bucket.PurgeExpired(now);

            if (bucket.DailyCount >= _maxPerDay)
            {
                var resetTime = bucket.DayStart.AddDays(1);
                var retryAfter = (int)Math.Ceiling((resetTime - now).TotalSeconds);
                return (false, 0, Math.Max(retryAfter, 1));
            }

            if (bucket.MinuteTimestamps.Count >= _maxPerMinute)
            {
                var oldest = bucket.MinuteTimestamps.Peek();
                var retryAfter = (int)Math.Ceiling((oldest.AddSeconds(60) - now).TotalSeconds);
                return (false, _maxPerDay - bucket.DailyCount, Math.Max(retryAfter, 1));
            }

            bucket.MinuteTimestamps.Enqueue(now);
            bucket.DailyCount++;

            return (true, _maxPerDay - bucket.DailyCount, 0);
        }
    }

    private sealed class IpBucket
    {
        public Queue<DateTimeOffset> MinuteTimestamps { get; } = new();
        public int DailyCount { get; set; }
        public DateTimeOffset DayStart { get; set; } = DateTimeOffset.UtcNow.Date;

        public void PurgeExpired(DateTimeOffset now)
        {
            // Reset daily counter at midnight UTC
            if (now.Date > DayStart.Date)
            {
                DailyCount = 0;
                DayStart = now.Date;
            }

            // Remove timestamps older than 60 seconds
            var cutoff = now.AddSeconds(-60);
            while (MinuteTimestamps.Count > 0 && MinuteTimestamps.Peek() < cutoff)
            {
                MinuteTimestamps.Dequeue();
            }
        }
    }
}
