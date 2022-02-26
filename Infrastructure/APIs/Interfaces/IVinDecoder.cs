namespace Infrastructure.APIs.Interfaces
{
    public interface IVinDecoder<T> where T : class
    {
        public bool Succes { get; set; }
        public Task<T> IdentifyCarByVINAsync (string vin);
        public string GetUri (string vin);
    }
}
