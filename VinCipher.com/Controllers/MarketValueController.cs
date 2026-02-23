using VinCipher.Extensions;
using VinCipher.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace VinCipher.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class MarketValueController : ControllerBase
{
    private readonly ICarService _carService;
    private readonly ICrudServices _requestsbase;
    private readonly TokensProvider _tokensProvider;
    private readonly IHttpConsumtionServices _httpConsumtionServices;
    public MarketValueController (ICarService carService, ICrudServices crudServices, TokensProvider tokensProvider, IHttpConsumtionServices httpConsumtionServices)
    {
        _carService = carService;
        _requestsbase = crudServices;
        _tokensProvider = tokensProvider;
        _httpConsumtionServices = httpConsumtionServices;
    }

    /// <summary>
    /// Recevoir une évalution d'une voiture en FCFA ainsi que les informations de bases si le véhicule a été identifié.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="vin"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<MarketValueResponse>> GetMarketValue (string token, string vin)
    {

        if (!_tokensProvider.IsValid(token, out var tokenInfo)
            || !tokenInfo.IsFunctionAllowed(AllowedFunction.Evaluate)
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
        int valeur = await _carService.FindSameCarValue(vin);

        var car =  await _httpConsumtionServices.FindCar(vin);
        if (car != null)
        {
            if(car.Year>= DateTime.Today.Year - 10)
            {
                valeur = car.MarketValue;
            }
        }
        if (valeur<400_000)
        {   
            if(car != null && car.MarketValue > valeur)
            {
               return Ok(new MarketValueResponse(new MarketValue(valeur)));
            }
            await _requestsbase.Ajouter(vin);

            //var data = DictionaryExtension.loadBinFile(vin);

            return NotFound(new MarketValueResponse());
        }

        MarketValue response = new MarketValue(valeur);
        return Ok(new MarketValueResponse(response));
    }


}

