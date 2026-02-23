using Microsoft.AspNetCore.Mvc;

using Services.DataServices;
using Services.Interfaces;

using VinCipher.Model;

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
        ILogger<PlaygroundController> logger)
    {
        _decoder = new VinDecoderController(tokensProvider, vinRushScrapper, context, crudServices, decoderRateLimiter);
        _playgroundToken = configuration["PlaygroundToken"]
            ?? throw new InvalidOperationException("PlaygroundToken is not configured.");
        _rateLimiter = rateLimiter;
        _challenge = challenge;
        _previewFieldCount = configuration.GetValue("Playground:PreviewFieldCount", 6);
        _logger = logger;
    }

    /// <summary>
    /// Decodes a VIN after verifying the HMAC challenge signature.
    /// </summary>
    [HttpGet("decode")]
    public async Task<ActionResult<Dictionary<string, string>>> Decode(
        [FromQuery] string vin,
        [FromQuery(Name = "t")] long timestamp,
        [FromQuery(Name = "s")] string signature)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return BadRequest("Le VIN est requis.");

        // HMAC challenge verification
        if (!_challenge.Verify(vin, timestamp, signature ?? ""))
        {
            _logger.LogWarning("Playground HMAC challenge failed for VIN {Vin}, t={Timestamp}", vin, timestamp);
            return StatusCode(403, new { error = "Signature invalide", message = "Requ\u00eate non autoris\u00e9e." });
        }

        // Rate limiting by client IP
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
                message = $"Vous avez atteint la limite de d\u00e9codages gratuits. R\u00e9essayez dans {FormatRetry(retryAfter)}.",
                retryAfterSeconds = retryAfter
            });
        }

        // Delegate to decoder
        var result = await _decoder.Decode(_playgroundToken, vin);

        // Truncate response
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
                preview["_demo"] = $"Aper\u00e7u limit\u00e9 \u2014 {hiddenCount} champs masqu\u00e9s. Souscrivez pour l\u2019acc\u00e8s complet.";

            preview["_remaining"] = remainingDaily.ToString();
            return Ok(preview);
        }

        return result;
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
