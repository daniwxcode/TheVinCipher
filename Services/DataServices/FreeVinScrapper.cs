using Infrastructure.APIs.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataServices
{
    public class FreeVinScrapper : IVinDecoder<Dictionary<string, string>>
    {
        private readonly IScrappableSource source;
        public bool Succes { get; set; }

        public Task<Dictionary<string, string>> IdentifyCarByVINAsync (string vin)
        {
            throw new NotImplementedException();
        }

        public string GetUri (string vin)
        {
            throw new NotImplementedException();
        }
    }
}
