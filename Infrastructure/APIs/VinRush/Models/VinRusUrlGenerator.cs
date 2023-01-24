using Infrastructure.APIs.Interfaces;

using Jint;

namespace Infrastructure.APIs.VinRush.Models
{
    public class VinRusUrlGenerator : IScrappableSource
    {
        

        public async Task<string> GetUrlAsync (string vin, int us =0)
        {
            //if (us == 0) return $"https://www.freevindecoder.eu/{vin}";
            var ulrPrefix = "https://www.vinrush.com/en/decode-check/";
            var engine = new Engine();
            var fromValue = engine.Execute(Javascript.Js);

            var code = engine.Invoke("skew", vin + "test").ToString().Substring(0, 5);
            return ulrPrefix + vin + '/' + code;
        }

    }

}
