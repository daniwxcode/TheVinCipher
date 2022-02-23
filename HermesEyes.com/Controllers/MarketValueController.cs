
using HermesEyes.com.Model;

using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace HermesEyes.com.Controllers;

[Route("api/[controller]/[Action]/{token}/")]
[ApiController]
public class MarketValueController : ControllerBase
{
    private readonly ICarService _service;
    private readonly ICrudServices requestsbase;
    public MarketValueController (ICarService carService, ICrudServices services)
    {
        _service = carService;
        requestsbase = services;
    }

    /// <summary>
    /// Recevoir une évalution d'une voiture en FCFA ainsi que les informations de bases si le véhicule a été identifié.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="vin"></param>
    /// <returns></returns>

    [HttpGet]
    [HttpPost]
    public async Task<ActionResult<MarketValueResponse>> GetMarketValue (string token, string vin)
    {

        var car = await _service.FindSameCar(vin);
        if (car == null || car.Vin == null)
        {
            requestsbase.Ajouter(vin);
            return NotFound(new MarketValueResponse());
        }
        return Ok(new MarketValueResponse(car));
    }
}

