
using HermesEyes.com.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Services.Interfaces;

namespace HermesEyes.com.Controllers;

    [Route("api/[controller]/[Action]/{Token}/")]
    [ApiController]
    public class MarketValueController : ControllerBase
    {
        private readonly ICarService _service;
        public MarketValueController( ICarService carService)
        {
            _service = carService;
        } 
        
    
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<MarketValueResponse>> GetMarketValue (string token, string vin)
        {
           
            var car = await _service.FindSameCar(vin);
            if(car == null || car.Vin == null)
            {
                return NotFound(new MarketValueResponse());
            }
            return Ok(new MarketValueResponse(car));
        }
    }

