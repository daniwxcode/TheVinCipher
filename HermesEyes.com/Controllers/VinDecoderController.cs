using Flurl.Http;

using HermesEyes.com.Model;

using Infrastructure.Contexts;

using Microsoft.AspNetCore.Mvc;

using Services.DataServices;
using Services.Interfaces;

using System.Text.RegularExpressions;

namespace HermesEyes.com.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VinDecoderController : ControllerBase
{
    private readonly VinRushScrapper vinRushScrapper;
    private readonly TokensProvider _tokensprovider;
    private readonly HermesContext _context;
    private readonly ICrudServices _requestsbase;
    public VinDecoderController (TokensProvider tokensProvider, VinRushScrapper vinRushScrapper, HermesContext context, ICrudServices crudServices)
    {
        this.vinRushScrapper = vinRushScrapper;
        _tokensprovider = tokensProvider;
        _context = context;
        _requestsbase = crudServices;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="vin"></param>
    /// <returns>Dictionary if OK code 200</returns>
    /// <returns>Bad Request</returns>
    /// <returns>Unauthorize</returns>
    /// <returns>NotFound</returns>
    [HttpGet()]
    [HttpPost]

    public async Task<ActionResult<Dictionary<string, string>>> Decode (string token, string vin)
    {
        if (token == null || !_tokensprovider.IsValid(token))
        {
            return Unauthorized(new MarketValueResponse("Token Invalid"));
        }
        if (vin.Contains("O") || vin.Contains("Q") || vin.Contains("I"))
        {
            var message = $"Vin Incorect: {vin}";
            await _requestsbase.Ajouter(message);
            return BadRequest(message);
        }

        vinRushScrapper.Vin = vin;
        var result = await vinRushScrapper.IdentifyCarByVINAsync(vin);
        result.TryAdd("model_year", vin.GetModelYear().ToString());
        result.Remove("note");
        result.Remove("adress_line_1");
        result.Remove("adress_line_2");
        if (result.Count <= 7)
        {
            result = await _requestUsBase(vin);
            result = await decodingParser(result);
            if (result.Count > 7)
            {
                return Ok(result);
            }
            await _requestsbase.Ajouter(vin);
            return NotFound(result);
        }
; return Ok(result);
    }

    private async Task<Dictionary<string, string>> _requestUsBase (string vin)
    {

        var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinextended/{vin}?format=json";
        var response = new Dictionary<string, string>();
        var result = await url.GetJsonAsync<VinDecodeRoot>();
        return response = result.Results.Where(i => i.Value != null).Select(r => new
        {
            label = Regex.Replace(r.Variable, @"[^0-9a-zA-Z:, /]+", "").Trim(),
            value = r.Value,
        }).ToList().ToDictionary(keySelector: m => m.label, elementSelector: m => m.value);
    }

    private async Task<Dictionary<string, string>> decodingParser (Dictionary<string, string> result)
    {
        foreach (var label in badlabels)
        {
            result.Remove(label);
        }
        foreach (var tuple in goodLabels)
        {
            RenameKey(result, tuple.toremane, tuple.good);
        }

        return result;
    }

    private readonly List<string> badlabels = new List<string>()
    {
         "Suggested VIN",
         "Error Code",
         "Possible Values",
         "Error Text",
         "NCSA Make",
         "NCSA Model",
         "Lane Departure Warning LDW",
         "Base Price",


    };
    private readonly List<(string good, string toremane)> goodLabels = new List<(string, string)>()
   {
        ("make","Make"),
        ("model","Model"),
        ("model_year","Model Year"),
        ("trim_level","Trim"),
        ("body_style","Body Class"),
        ("fuel_type","Fuel Type  Primary"),
        ("transmission","Transmission Style"),
        ("manufactured_in","Plant City"),
        ("manufacturer","Manufacturer Name"),
        ("country","Plant Country"),
        ("body_type","Body Class"),
        ("number_of_doors","Doors"),
        ("number_of_seats","Number of Seats"),
        ("displacement_si","Displacement CC"),
        ("displacement_nominal","Displacement L"),
        ("engine_head","Engine Configuration"),
        ("engine_cylinders","Engine Number of Cylinders"),
        ("engine_horse_power","Engine Brake hp From"),
        ("engine_kilo_watts","Engine Power kW"),
        ("vehicule_type","Vehicle Type"),
        ("driveline","Drive Type"),

    };

    private void RenameKey (Dictionary<string, string> dic,
                                      string fromKey, string toKey)
    {
        try
        {
            var value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
        catch (Exception ex)
        {

        }

    }
}
