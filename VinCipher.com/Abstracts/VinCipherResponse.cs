using Domaine.Abstracts;


namespace VinCipher.Abstracts
{
    public abstract class VinCipherResponse<T> where T : new()
    {
        public DateTime ResponseDate { get; init; } = DateTime.Now;
        public string Message { get; set; }
        public bool Success { get; set; } = false;
        public T Data { get; set; }
       // public int MarketValue { get; set; }


    }
}
