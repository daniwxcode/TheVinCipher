
using Domaine.Entities;

using HermesEyes.com.Model;

using Infrastructure.APIs.Abstracts;

using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace HermesEyes.com.Controllers;

[Route("api/[controller]/[Action]/{token}/")]
[ApiController]
public class MarketValueController : ControllerBase
{
    private readonly ICarService _service;
    private readonly ICrudServices requestsbase;
    private readonly TokensProvider _tokensprovider;
    private readonly IHttpConsumtionServices _httpclient;
    private readonly BaseApiProvider _BaseApiProvider;
    public MarketValueController (ICarService carService, ICrudServices services, TokensProvider tokensProvider, IHttpConsumtionServices http, BaseApiProvider baseApiProvider)
    {
        _service = carService;
        requestsbase = services;
        _tokensprovider = tokensProvider;
        _httpclient = http;
        _BaseApiProvider = baseApiProvider;
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
            var carBase = await _httpclient.FindCar(vin);
            if (carBase == null||carBase.Vin==null || carBase.Make ==null)
            {
                Console.WriteLine(carBase.Model);
                await requestsbase.Ajouter(vin);
                return NotFound(new MarketValueResponse());
            }           
        return Ok(new MarketValueResponse(new CarDecode(carBase)));
    }
}

