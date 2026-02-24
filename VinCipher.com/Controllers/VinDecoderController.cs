using System.Collections.Frozen;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Services.DataServices;

using VinCipher.Model;
using VinCipher.Model.Playground;
using VinCipher.Services;

namespace VinCipher.Controllers;

[Route("api/[controller]")]
[ApiController]
public partial class VinDecoderController : ControllerBase
{
    private readonly VinRushScrapper _scrapper;
    private readonly VinCypScrapper _vinCyp;
    private readonly FreeVinDecoderScrapper _freeVin;
    private readonly TokensProvider _tokenProvider;
    private readonly VinDecoderRateLimiter _rateLimiter;
    private readonly VinDecodeCache _vinCache;
    private readonly PlaygroundDbContext? _pgDb;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdminEventBus? _eventBus;

    public VinDecoderController(
        TokensProvider tokensProvider,
        VinRushScrapper vinRushScrapper,
        VinCypScrapper vinCypScrapper,
        FreeVinDecoderScrapper freeVinDecoderScrapper,
        VinDecoderRateLimiter rateLimiter,
        VinDecodeCache vinCache,
        IHttpClientFactory httpClientFactory,
        PlaygroundDbContext? pgDb = null,
        AdminEventBus? eventBus = null)
    {
        _scrapper = vinRushScrapper;
        _vinCyp = vinCypScrapper;
        _freeVin = freeVinDecoderScrapper;
        _tokenProvider = tokensProvider;
        _rateLimiter = rateLimiter;
        _vinCache = vinCache;
        _httpClientFactory = httpClientFactory;
        _pgDb = pgDb;
        _eventBus = eventBus;
    }

    /// <summary>
    /// Decodes a VIN and returns vehicle specifications.
    /// Uses in-memory cache (24h) to avoid redundant scraping.
    /// </summary>
    [HttpGet]
    [HttpPost]
    public async Task<ActionResult<Dictionary<string, string>>> Decode(string token, string vin, CancellationToken cancellationToken)
    {
        if (!_tokenProvider.IsValid(token, out var tokenInfo)
            || !tokenInfo.IsFunctionAllowed(AllowedFunction.Decode))
        {
            return Unauthorized(new MarketValueResponse("Token Invalid"));
        }

        if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
            return BadRequest(new { error = "VIN invalide", message = "Le VIN doit contenir exactement 17 caractères." });

        vin = vin.Trim().ToUpperInvariant();

        var (allowed, _, retryAfter) = _rateLimiter.TryAcquire(token);
        if (!allowed)
        {
            Response.Headers["Retry-After"] = retryAfter.ToString();
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Daily limit of 50 decode requests reached.", retryAfterSeconds = retryAfter });
        }

        if (vin.Contains('O') || vin.Contains('Q') || vin.Contains('I'))
        {
            return BadRequest(new { error = "VIN invalide", message = "Le VIN contient des caractères interdits (O, Q, I)." });
        }

        var startTime = Stopwatch.GetTimestamp();

        // Return cached result if available (24h TTL)
        string provider;
        ActionResult<Dictionary<string, string>> result;
        if (_vinCache.TryGet(vin[..11], out var cachedResult))
        {
            result = Ok(cachedResult);
            provider = "Cache";
        }
        else
        {
            (result, provider) = await ScrapeAndCacheAsync(vin, cancellationToken);
        }

        var elapsedMs = (int)Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
        await LogRequestAsync(token, vin, result, elapsedMs, provider, cancellationToken);

        return result;
    }

    /// <summary>
    /// Logs the request to PlaygroundDbContext if the token maps to a playground API token.
    /// Skipped when pgDb is null (i.e. when called from PlaygroundController which has its own logging).
    /// </summary>
    private async Task LogRequestAsync(string token, string vin, ActionResult<Dictionary<string, string>> result, int elapsedMs, string provider, CancellationToken cancellationToken)
    {
        if (_pgDb is null) return;

        var apiToken = await _pgDb.ApiTokens
            .FirstOrDefaultAsync(t => t.Key == token && t.IsActive, cancellationToken);

        if (apiToken is null) return;

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
            TokenId = apiToken.Id,
            Vin = vin,
            StatusCode = statusCode,
            Success = statusCode is >= 200 and < 300,
            ResponseTimeMs = elapsedMs,
            Provider = provider
        });

        await _pgDb.SaveChangesAsync(cancellationToken);

        _eventBus?.PublishRequestLogged(vin, statusCode, statusCode is >= 200 and < 300, elapsedMs, provider);
    }

    private async Task<(ActionResult<Dictionary<string, string>> Result, string Provider)> ScrapeAndCacheAsync(string vin, CancellationToken cancellationToken)
    {
        // 1. VinRush (primary)
        var result = await _scrapper.IdentifyCarByVINAsync(vin);
        result.TryAdd("model_year", vin.GetModelYear().ToString());

        if (result.Count > 17)
        {
            result.Remove("Base Price");
            _vinCache.Set(vin[..11], result);
            return (Ok(result), "VinRush");
        }

        // 2. NHTSA (US federal database)
        result = await RequestUsBaseAsync(vin, cancellationToken);
        ParseAndClean(result);

        if (result.Count >= 17)
        {
            _vinCache.Set(vin[..11], result);
            return (Ok(result), "NHTSA");
        }

        // 3. VinRush US mode
        result = await _scrapper.IdentifyCarByVINAsync(vin, 1);
        ParseAndClean(result);

        if (result.Count > 17)
        {
            _vinCache.Set(vin[..11], result);
            return (Ok(result), "VinRush-US");
        }

        // 4. VinCyP
        var cypResult = await _vinCyp.DecodeAsync(vin, cancellationToken);
        if (cypResult.Count > 0)
        {
            ParseAndClean(cypResult);
            cypResult.TryAdd("model_year", vin.GetModelYear().ToString());

            if (cypResult.Count > 5)
            {
                _vinCache.Set(vin[..11], cypResult);
                return (Ok(cypResult), "VinCyP");
            }
        }

        // 5. FreeVinDecoder.eu (last resort)
        var freeResult = await _freeVin.DecodeAsync(vin, cancellationToken);
        if (freeResult.Count > 0)
        {
            ParseAndClean(freeResult);
            freeResult.TryAdd("model_year", vin.GetModelYear().ToString());

            if (freeResult.Count > 5)
            {
                _vinCache.Set(vin[..11], freeResult);
                return (Ok(freeResult), "FreeVinDecoder");
            }
        }

        return (NotFound(result), "NotFound");
    }

    private async Task<Dictionary<string, string>> RequestUsBaseAsync(string vin, CancellationToken cancellationToken)
    {
        var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinextended/{vin}?format=json";
        using var client = _httpClientFactory.CreateClient();
        var decoded = await client.GetFromJsonAsync<VinDecodeRoot>(url, cancellationToken);

        if (decoded?.Results is null)
            return new Dictionary<string, string>();

        var response = new Dictionary<string, string>(decoded.Results.Count);
        foreach (var r in decoded.Results)
        {
            if (r.Value is null) continue;
            var label = NonAlphaNumRegex().Replace(r.Variable, "").Trim();
            response.TryAdd(label, r.Value);
        }

        return response;
    }

    private static void ParseAndClean(Dictionary<string, string> result)
    {
        var keysToRemove = result
            .Where(p => p.Value == "Not Applicable" || BadLabels.Contains(p.Key))
            .Select(p => p.Key)
            .ToList();

        foreach (var key in keysToRemove)
            result.Remove(key);

        foreach (var (originalKey, goodKey) in RenameMap)
        {
            if (result.Remove(originalKey, out var value))
                result[goodKey] = value;
        }

        foreach (var key in result.Keys.ToList())
        {
            var slashIndex = result[key].IndexOf('/');
            if (slashIndex >= 0)
                result[key] = result[key][..slashIndex];
        }

        result.Remove("notea");
        result.Remove("adress_line_1");
        result.Remove("adress_line_2");
    }

    [GeneratedRegex(@"[^0-9a-zA-Z:, /]+")]
    private static partial Regex NonAlphaNumRegex();

    private static readonly FrozenSet<string> BadLabels = FrozenSet.ToFrozenSet(
    [
        "Suggested VIN", "Error Code", "Possible Values", "Error Text",
        "NCSA Make", "NCSA Model", "Lane Departure Warning LDW", "Base Price",
        "Additional Error Text", "Motorcycle Chassis Type", "image", "exec"
    ]);

    /// <summary>
    /// Maps original key → standardized key.
    /// </summary>
    private static readonly FrozenDictionary<string, string> RenameMap =
        new Dictionary<string, string>
        {
            ["Make"] = "make",
            ["brand"] = "make",
            ["Model"] = "model",
            ["Model Year"] = "model_year",
            ["year"] = "model_year",
            ["Trim"] = "trim_level",
            ["Body Class"] = "body_style",
            ["Fuel Type  Primary"] = "fuel_type",
            ["Transmission Style"] = "transmission",
            ["Plant City"] = "manufactured_in",
            ["Manufacturer Name"] = "manufacturer",
            ["Plant Country"] = "country",
            ["Doors"] = "number_of_doors",
            ["Number of Seats"] = "number_of_seats",
            ["number_of_seater"] = "number_of_seats",
            ["Displacement CC"] = "displacement_si",
            ["Displacement L"] = "displacement_nominal",
            ["Engine Configuration"] = "engine_head",
            ["Engine Number of Cylinders"] = "engine_cylinders",
            ["Engine Brake hp From"] = "engine_horse_power",
            ["Engine Power kW"] = "engine_kilo_watts",
            ["Vehicle Type"] = "vehicule_type",
            ["Drive Type"] = "driveline",
        }.ToFrozenDictionary();
}
