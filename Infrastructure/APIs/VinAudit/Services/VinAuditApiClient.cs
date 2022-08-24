using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinAudit.Models;
using Infrastructure.APIs.VinAudit.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.APIs.VinCario.Services
{
    public class VinAuditApiClient : BaseApiProviderClient<CarBase>, IVinDecoder<VinAuditResponse>
    {
        public VinAuditApiClient (HttpClient httpClient, BaseApiProvider apiProvider) : base(httpClient, apiProvider)
        {
        }

        public bool Succes { get; set; } = false;

        public async Task<VinAuditResponse> IdentifyCarByVINAsync (string vin, int us = 0)
        {
            VinAuditResponse vinAuditResponse = null;
            try
            {
                var url = GetUri(vin);
                string response = await _httpClient.GetStringAsync(url);
                if (response != null)
                {
                    vinAuditResponse = JsonSerializer.Deserialize<VinAuditResponse>(response);
                }
                return vinAuditResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        public override async Task<CarBase> GetResult (string vin)
        {
            var response = await IdentifyCarByVINAsync(vin);
            if (response==null|| !response.Success)
            {
                throw new KeyNotFoundException("Vin decoder : Impossible de décoder ce vin");
            }
            var result = new VinAuditToCarBaseConverter(response).NewCar;

            return result;

        }

        public string GetUri (string vin, int us=0)
        {
            var url = $"{apiProvider.ApiUrlPrefix}key={apiProvider.ApiKeyToken}&format=json&vin={vin}";
            return url;
        }
    }
}
