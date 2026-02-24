using System.Text.Json;
using System.Threading.Channels;

namespace VinCipher.Services;

/// <summary>
/// Lightweight in-memory event bus for SSE broadcasting to admin dashboards.
/// Each connected admin gets its own Channel; events are fanned out to all.
/// </summary>
public sealed class AdminEventBus
{
    private readonly Lock _lock = new();
    private readonly List<Channel<AdminEvent>> _subscribers = [];

    public ChannelReader<AdminEvent> Subscribe()
    {
        var ch = Channel.CreateBounded<AdminEvent>(new BoundedChannelOptions(64)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        lock (_lock) { _subscribers.Add(ch); }
        return ch.Reader;
    }

    public void Unsubscribe(ChannelReader<AdminEvent> reader)
    {
        lock (_lock)
        {
            _subscribers.RemoveAll(ch => ch.Reader == reader);
        }
    }

    public void Publish(AdminEvent evt)
    {
        lock (_lock)
        {
            foreach (var ch in _subscribers)
                ch.Writer.TryWrite(evt);
        }
    }

    /// <summary>
    /// Shortcut: publish a request-logged event after a VIN decode.
    /// </summary>
    public void PublishRequestLogged(string vin, int statusCode, bool success, int responseTimeMs, string provider)
    {
        Publish(new AdminEvent("request-logged", new
        {
            vin,
            statusCode,
            success,
            responseTimeMs,
            provider,
            timestamp = DateTime.UtcNow
        }));
    }
}

public sealed record AdminEvent(string Type, object Data)
{
    public string ToSseData()
    {
        var json = JsonSerializer.Serialize(new { type = Type, data = Data });
        return $"data: {json}\n\n";
    }
}
