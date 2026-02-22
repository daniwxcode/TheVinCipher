using Domaine.Entities.VinCipher;

using Flurl.Http;

using VinCipher.Model;

using Infrastructure.Contexts;

using Microsoft.AspNetCore.Mvc;

using Services.DataServices;
using Services.Interfaces;

using System.Text.RegularExpressions;

namespace VinCipher.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VinDecoderController : ControllerBase
{
    private readonly VinRushScrapper vinRushScrapper;
    private readonly TokensProvider tokenProvider;
    private readonly VinCipherContext _context;
    private readonly ICrudServices _requestsbase;
    public VinDecoderController(TokensProvider tokensProvider, VinRushScrapper vinRushScrapper, VinCipherContext context, ICrudServices crudServices)
    {
        this.vinRushScrapper = vinRushScrapper;
        tokenProvider = tokensProvider;
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

    public async Task<ActionResult<Dictionary<string, string>>> Decode(string token, string vin)
    {

        if (!tokenProvider.IsValid(token, out var tokenInfo)
            || !tokenInfo.IsFunctionAllowed(AllowedFunction.Decode)
            )
        {
            return Unauthorized(new MarketValueResponse("Token Invalid"));
        }
        if (vin.Contains("O") || vin.Contains("Q") || vin.Contains("I"))
        {
            var message = $"Vin Incorect: {vin}";
            await _requestsbase.Ajouter(message);
            return BadRequest(message);
        }
        try
        {
            var existingCar = _context.VinCipherCars.FirstOrDefault(c => c.VIN == vin);

            if (existingCar != null)
            {
                return Ok(existingCar.DecodedValues);
            }
        }
        catch (Exception)
        {

        }

        vinRushScrapper.Vin = vin;
        var result = await vinRushScrapper.IdentifyCarByVINAsync(vin);
        result.TryAdd("model_year", vin.GetModelYear().ToString());
        if (result.Count <= 17)
        {
            result = await RequestUsBase(vin);
            result = DecodingParser(result);
            if (result.Count < 17)
            {
                result = await vinRushScrapper.IdentifyCarByVINAsync(vin, 1);
                result = DecodingParser(result);
                if (result.Count > 17)
                    return Ok(result);

                await _requestsbase.Ajouter(vin);
                return NotFound(result);
            }


        }
        var hermescar = new VinCipherCar(result, vin);
        try
        {
            await _context.VinCipherCars.AddAsync(hermescar);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {

        }
        result.Remove("Base Price");
        return Ok(result);

    }

    private async Task<Dictionary<string, string>> RequestUsBase(string vin)
    {

        var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinextended/{vin}?format=json";
        var response = new Dictionary<string, string>();
        var result = await url.GetJsonAsync<VinDecodeRoot>();
        var res = result.Results.Where(i => i.Value != null).Select(r => new
        {
            label = Regex.Replace(r.Variable, @"[^0-9a-zA-Z:, /]+", "").Trim(),
            value = r.Value,
        }).ToList();
        foreach (var item in res)
        {
            response.TryAdd(item.label, item.value);
        }
        return response;
    }

    private Dictionary<string, string> DecodingParser(Dictionary<string, string> result)
    {
        foreach (var pair in result)
        {
            if (pair.Value == "Not Applicable")
            {
                result.Remove(pair.Key);

            }
        }

        foreach (var label in badlabels)
        {
            result.Remove(label);
        }
        foreach (var tuple in goodLabels)
        {
            RenameKey(result, tuple.toremane, tuple.good);
        }
        foreach (var item in result.Keys)
        {
            var tmp = result[item].Split('/');
            if (tmp.Length > 1)
            {
                result[item] = tmp[0];
            }
        }

        result.Remove("notea");
        result.Remove("adress_line_1");
        result.Remove("adress_line_2");
        return result;
    }

    private readonly List<string> badlabels = new()
    {
         "Suggested VIN",
         "Error Code",
         "Possible Values",
         "Error Text",
         "NCSA Make",
         "NCSA Model",
         "Lane Departure Warning LDW",
         "Base Price",
         "Additional Error Text",
         "Motorcycle Chassis Type",
         "image",
         "exec"


    };
    private readonly List<(string good, string toremane)> goodLabels = new()
   {
        ("make","Make"),
        ("make","brand"),
        ("model","Model"),
        ("model_year","Model Year"),
        ("model_year","year"),
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
        ("number_of_seats","number_of_seater"),
        ("displacement_si","Displacement CC"),
        ("displacement_nominal","Displacement L"),
        ("engine_head","Engine Configuration"),
        ("engine_cylinders","Engine Number of Cylinders"),
        ("engine_horse_power","Engine Brake hp From"),
        ("engine_kilo_watts","Engine Power kW"),
        ("vehicule_type","Vehicle Type"),
        ("driveline","Drive Type"),

    };

    private void RenameKey(Dictionary<string, string> dic,
                                      string fromKey, string toKey)
    {
        try
        {
            var value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
        catch (Exception)
        {

        }

    }
}
