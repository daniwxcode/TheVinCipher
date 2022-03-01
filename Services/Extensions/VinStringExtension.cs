using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Extensions
{
    public static class VinStringExtension
    {
        public static string US_WMI ( this string Vin ){return Vin.Substring(11, 3); }
        public static string US_SERIAL ( this string Vin ){return Vin.Substring(14, 3); }
        public static string WMI ( this string Vin ){return Vin.Substring(0, 3); }
        public static string VDS ( this string Vin ){return Vin.Substring(3, 6); }
        public static string COMPO ( this string Vin ){return Vin.Substring(3, 5); }
        public static string EQUIP ( this string Vin ){return Vin.Substring(8, 1); }
        public static string YEAR ( this string Vin ){return Vin.Substring(9, 1); }
        public static string SERIAL ( this string Vin ){return Vin.Substring(10, 6); }
        public static string CODE ( this string Vin ){return Vin.Substring(10, 1); }
    }
}
