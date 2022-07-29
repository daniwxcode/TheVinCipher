using Infrastructure.APIs.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.APIs.FreeVin
{
    public class FreeVinDecoderUrlGenerator : IScrappableSource
    {
        public async Task<string> GetUrlAsync (string vin)
        {
            return $"https://www.freevindecoder.eu/{vin}";
        }
    }
}
