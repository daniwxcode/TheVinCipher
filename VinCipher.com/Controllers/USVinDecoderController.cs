using Flurl.Http;

using HermesEyes.com.Model;

using Infrastructure.Contexts;

using Microsoft.AspNetCore.Mvc;

using Services.DataServices;
using Services.Interfaces;

using System.Text.RegularExpressions;

namespace HermesEyes.com.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class USVinDecoderController : ControllerBase
    {
        public USVinDecoderController ()
        {
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
            var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinextended/{vin}?format=json";
            var response = new Dictionary<string, string>();
            var result = await url.GetJsonAsync<VinDecodeRoot>();
            response = result.Results.Where(i => i.Value != null).Select(r => new
            {
                label = Regex.Replace(r.Variable, @"[^0-9a-zA-Z:, /]+", "").Trim(),
                value = r.Value,
            }).ToList().ToDictionary(keySelector: m => m.label, elementSelector: m => m.value);
            return Ok(response);

        }
    }
}
