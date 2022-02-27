using Domaine.Entities;

using Infrastructure.Contexts;

using Services.Interfaces;

namespace Services.DataServices
{
    public class DecodedCarBaseService : IHttpConsumtionServices
    {
        private readonly HermesContext _dbContext;
        // private readonly VincarioApiClient _api;
        public DecodedCarBaseService (HermesContext hermesContext)//, VincarioApiClient vincarioApi)
        {
            _dbContext = hermesContext;
            // _api = vincarioApi;
        }

        public async Task<CarBase> FindCar (string vin)
        {
            var carbase = _dbContext.CarsBase.FirstOrDefault();
            if (carbase != null)
            {
                return carbase;

            }
            return null;
            //  return await _api.GetResult(vin);
        }
    }
}
