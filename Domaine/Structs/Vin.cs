namespace Domaine.Structs
{
    public struct Vin
    {
        public Vin (string vin)
        {
            if (string.IsNullOrEmpty(vin) || vin.Trim().Length != 17)
            {
                throw new ArgumentException("Impossible de traiter ce vin");
            }
            vin = vin.ToUpper();

            if (vin[3] == '9')
            {
                US = true;
                US_WMI = vin.Substring(12, 3);
                US_SERIAL = vin.Substring(15, 3);
            }
            WMI = vin.Substring(0, 3);
            VDS = vin.Substring(3, 6);
            COMPO = vin.Substring(3, 5);
            EQUIP = vin.Substring(8, 1);
            YEAR = vin.Substring(9, 1);
            SERIAL = vin.Substring(10, 6);
            CODE = vin.Substring(10, 1);
        }
        public string WMI { get; set; }
        public string VDS { get; set; }
        public string COMPO { get; set; }
        public string EQUIP { get; set; }
        public string YEAR { get; set; }
        public string CODE { get; set; }
        public string SERIAL { get; set; }
        public bool US { get; set; } = false;
        public string US_WMI { get; set; } = String.Empty;
        public string US_SERIAL { get; set; } = String.Empty;

        public bool HasSameWMIWith (Vin vin)
        {

            return vin.WMI == WMI;
        }
        public bool IsSameModelandYearWith (Vin vin)
        {
            return vin.VDS == VDS;
        }
        public bool HasSameYearModelWith (Vin vin)
        {
            return vin.YEAR == YEAR;
        }

    }
}
