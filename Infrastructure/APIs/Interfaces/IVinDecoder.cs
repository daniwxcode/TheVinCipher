namespace Infrastructure.APIs.Interfaces
{
    public interface IVinDecoder<T> where T : class
    {
        public bool Succes { get; set; }
        public Task<T> IdentifyCarByVINAsync (string vin, int us = 0);
        public string GetUri (string vin, int us = 0);
    }
}
