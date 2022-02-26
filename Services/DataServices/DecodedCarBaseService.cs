using Domaine.Entities;

using Infrastructure.APIs.VinCario.Services;
using Infrastructure.Contexts;

using Services.Abstract;
using Services.Interfaces;

namespace Services.DataServices
{
    public class DecodedCarBaseService : DatabaseService, IHttpConsumtionServices
    {
        private readonly VincarioApiClient _api;
        public DecodedCarBaseService (HermesContext hermesContext, VincarioApiClient vincarioApi) : base(hermesContext)
        {
            _api = vincarioApi;
        }

        public async Task<CarBase> FindCar (string vin)
        {
            var carbase = _dbContext.CarsBase.FirstOrDefault();
            if (carbase != null)
            {
                return carbase;

            }
            return await _api.GetResult(vin);
        }
    }
}
