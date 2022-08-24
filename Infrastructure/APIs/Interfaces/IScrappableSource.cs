using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.APIs.Interfaces
{
    public interface IScrappableSource
    {
        public Task<string> GetUrlAsync (string vin, int us =0);
    }
}
