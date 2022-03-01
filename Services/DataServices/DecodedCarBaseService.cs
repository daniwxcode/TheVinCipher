using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinCario.Models;
using Infrastructure.APIs.VinCario.Services;
using Infrastructure.Contexts;

using Services.Extensions;
using Services.Interfaces;

namespace Services.DataServices
{
    public class DecodedCarBaseService : IHttpConsumtionServices
    {
        private readonly HermesContext _dbContext;
        private readonly BaseApiProviderClient<CarBase> _api;
        private readonly BaseApiProvider _baseApiProvider;
        public DecodedCarBaseService (HermesContext hermesContext, HttpClient httpClient, BaseApiProvider apiProvider, BaseApiProviderClient<CarBase> client)
        {
            _dbContext = hermesContext;
            _baseApiProvider = apiProvider;

            _api = client;
        }

        public async Task<CarBase> FindCar (string vin)
        {
            CarBase carbase = null;
            if (vin.Length == 17)
            {

                carbase = _dbContext.CarsBase.FirstOrDefault(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11) || c.Vin.Substring(3, 6) == vin.Substring(3,6));
                if (carbase != null)
                {
                    return carbase;

                }
                // return null;
                carbase = await _api.GetResult(vin);
                if (carbase != null)
                {
                    _dbContext.Add(carbase);
                    await _dbContext.SaveChangesAsync();

                }

            }

            return carbase ?? new CarBase();
        }
    }
}
    
