using HermesEyes.com.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace HermesEyes.com.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarketValueController : ControllerBase
{
    private readonly ICarService _carService;
    private readonly ICrudServices _requestsbase;
    private readonly TokensProvider _tokensprovider;
    public MarketValueController (ICarService carService, ICrudServices crudServices, TokensProvider tokensProvider)
    {
        _carService = carService;
        _requestsbase = crudServices;
        _tokensprovider = tokensProvider;
    }

    /// <summary>
    /// Recevoir une évalution d'une voiture en FCFA ainsi que les informations de bases si le véhicule a été identifié.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="vin"></param>
    /// <returns></returns>
    [HttpGet()]
    [HttpPost]
    public async Task<ActionResult<MarketValueResponse>> GetMarketValue (string token, string vin)
    {
        if (token == null || !_tokensprovider.IsValid(token))
        {
            return Unauthorized(new MarketValueResponse("Token Invalid"));
        }
        int valeur= await _carService.FindSameCarValue(vin);
        if (valeur<500000)
        {
            //Console.WriteLine(carBase.Model);
            await _requestsbase.Ajouter(vin);
            return NotFound(new MarketValueResponse());
        }
        MarketValue response = new MarketValue(valeur);
        return Ok(new MarketValueResponse(response));
    }
}

