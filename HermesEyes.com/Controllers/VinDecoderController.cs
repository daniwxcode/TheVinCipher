using HermesEyes.com.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Services.DataServices;

namespace HermesEyes.com.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VinDecoderController : ControllerBase
    {
        private readonly VinRushScrapper vinRushScrapper;
        private readonly TokensProvider _tokensprovider;
        public VinDecoderController(TokensProvider tokensProvider, VinRushScrapper vinRushScrapper)
        {
            this.vinRushScrapper = vinRushScrapper;
            _tokensprovider = tokensProvider;   

        }
        [HttpGet()]
        [HttpPost]
        public async Task<ActionResult<Dictionary<string, string>>> Decode (string token, string vin)
        {
            if (token == null || !_tokensprovider.IsValid(token))
            {
                return Unauthorized(new MarketValueResponse("Token Invalid"));
            }
            vinRushScrapper.Vin = vin;
            var result = vinRushScrapper.IdentifyCarByVINAsync(vin);
           
;           return Ok (result);
        }
    }
}
