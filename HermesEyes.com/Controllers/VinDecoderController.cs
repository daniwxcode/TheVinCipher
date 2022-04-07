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

        public VinDecoderController(VinRushScrapper vinRushScrapper)
        {
            this.vinRushScrapper = vinRushScrapper;
        }
        [HttpGet()]
        [HttpPost]
        public async Task<ActionResult<Dictionary<string, string>>> Decode (string vin)
        {
            vinRushScrapper.Vin = vin;
            return Ok (vinRushScrapper.IdentifyCarByVINAsync(vin));
        }
    }
}
