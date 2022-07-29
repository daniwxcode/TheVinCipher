using HermesEyes.com.Model;

using Infrastructure.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Services.DataServices;
using Services.Interfaces;

namespace HermesEyes.com.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VinDecoderController : ControllerBase
    {
        private readonly VinRushScrapper vinRushScrapper;
        private readonly TokensProvider _tokensprovider;
        private readonly HermesContext _context;
        private readonly ICrudServices _requestsbase;
        public VinDecoderController(TokensProvider tokensProvider, VinRushScrapper vinRushScrapper, HermesContext context, ICrudServices crudServices)
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
            var result =await  vinRushScrapper.IdentifyCarByVINAsync(vin);            
            result.TryAdd("model_year", vin.GetModelYear().ToString());
            result.Remove("note");
            result.Remove("adress_line_1");
            result.Remove("adress_line_2");
            if(result.Count <=7)
            {
                await _requestsbase.Ajouter(vin);
                return NotFound(result);
            }
;           return Ok(result);
        }
    }
}
