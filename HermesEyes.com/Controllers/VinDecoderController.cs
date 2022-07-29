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
            if (result.Result.Count < 25)
            {
                await _requestsbase.Ajouter(vin);
            }
            if(result.Result.Count < 15)
            {
                return NotFound(result);
            }
;           return Ok (result);
        }
    }
}
