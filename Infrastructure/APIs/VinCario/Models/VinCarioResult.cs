namespace Infrastructure.APIs.VinCario.Models
{
    public class VinCarioResult
    {
        public int price { get; set; }
        public string price_currency { get; set; }
        public Balance balance { get; set; }
        public List<Decode> decode { get; set; }
    }
}