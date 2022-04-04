using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Extensions;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinCario.Models;

using System.Text.Json;

namespace Infrastructure.APIs.VinCario.Services
{
    public class VincarioApiClient : BaseApiProviderClient<CarBase>, IVinDecoder<VinCarioResult>
    {
        
        public VincarioApiClient (HttpClient httpClient, BaseApiProvider apiProvider) : base(httpClient, apiProvider)
        {
           
        }

        public bool Succes { get; set; } = false;

        public async Task<VinCarioResult> IdentifyCarByVINAsync (string vin)
        {
            VinCarioResult vinCarioResult= null;
            try
            {
                var url = GetUri(vin);
                string response = await _httpClient.GetStringAsync(url);
                if (response != null)
                {
                   vinCarioResult= JsonSerializer.Deserialize<VinCarioResult>(response);
                    //return _vinDecoderResult;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            return vinCarioResult;
        }
        private string controlrsum (string vin)
        {

            var text = $"{vin}|decode|{apiProvider.ApiKeyToken}|{apiProvider.ApiSecretKey}";
            return text.HmacSHA256().Substring(0, 10);

        }

        public string GetUri (string vin)
        {
            string? controlsum = controlrsum(vin);
            return $"{apiProvider.ApiUrlPrefix }/{apiProvider.ApiKeyToken}/{controlsum}/decode/{vin}.json";

        }
        public override async Task<CarBase> GetResult (string vin)
        {
            var response = await IdentifyCarByVINAsync(vin);
            if (response.decode == null)
            {
                throw new KeyNotFoundException("Vin decoder : Impossible de décoder ce vin");
            }
            var result = new VinCarioToCarBaseConverter(response).NewCar;

            return result ;

        }

    }
}
