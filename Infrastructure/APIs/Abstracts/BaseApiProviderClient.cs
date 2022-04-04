namespace Infrastructure.APIs.Abstracts
{
    public abstract class BaseApiProviderClient<T>
    {
        protected readonly HttpClient _httpClient;
        protected readonly BaseApiProvider apiProvider;
        public BaseApiProviderClient (HttpClient httpClient, BaseApiProvider apiProvider)
        {
            _httpClient = httpClient;
            this.apiProvider = apiProvider;
        }
       
        public virtual async Task<T> GetResult (string vin)
        {
            throw new NotImplementedException();
        }

    }
}
