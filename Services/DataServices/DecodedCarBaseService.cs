using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.APIs.Interfaces;
using Infrastructure.APIs.VinCario.Models;
using Infrastructure.APIs.VinCario.Services;
using Infrastructure.Contexts;

using Services.Extensions;
using Services.Interfaces;

using System.Globalization;

namespace Services.DataServices
{
    public class DecodedCarBaseService : IHttpConsumtionServices
    {
        private readonly HermesContext _dbContext;
        private readonly BaseApiProviderClient<CarBase> _api;
        private readonly BaseApiProvider _baseApiProvider;
        private readonly ICarService _carService;
        public DecodedCarBaseService (HermesContext hermesContext, HttpClient httpClient, BaseApiProvider apiProvider, BaseApiProviderClient<CarBase> client, ICarService carService)
        {
            _dbContext = hermesContext;
            _baseApiProvider = apiProvider;
            _carService = carService;
            _api = client;
        }

        public async Task<CarBase> FindCar (string vin)
        {
            CarBase carbase = null;
            if (vin.Length == 17)
            {

                carbase = _dbContext.CarsBases.FirstOrDefault(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11) || c.Vin.Substring(3, 6) == vin.Substring(3, 6));
                if (carbase != null)
                {
                    carbase.Vin = vin;
                    carbase = await OptimizeCarInfo(carbase);
                    try
                    {
                        _dbContext.CarsBases.Update(carbase);
                        _dbContext.SaveChanges();
                    }catch (Exception ex)
                    {

                    }


                    return carbase;

                }

                carbase = await _api.GetResult(vin);
                carbase = await OptimizeCarInfo(carbase);
                if (carbase != null)
                {
                    var car = _dbContext.CarsBases.FirstOrDefault(c => c.Vin == vin);
                    if (car == null)
                    {
                        _dbContext.CarsBases.Add(carbase);
                        await _dbContext.SaveChangesAsync();
                    }
                }

            }

            return carbase ?? new CarBase();
        }

        private async Task<CarBase> OptimizeCarInfo (CarBase carBase)
        {
            var vin = carBase.Vin;
            var Sameknown = _dbContext.Cars.Where(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11) || c.Vin.Substring(3, 6) == vin.Substring(3, 6));
            if (carBase.Year == null || carBase.Year == 0)
            {
                carBase.Year = Sameknown.FirstOrDefault(c => c.Year != 0 && c.Vin.Substring(9, 1) == vin.Substring(9, 1))?.Year;
            }
            if (carBase.FuelType == null || carBase.FuelType == String.Empty)
            {
                carBase.FuelType = Sameknown.FirstOrDefault(c => c.Energy != null)?.Energy;
            }
            if (carBase.EngineSize == null || carBase.EngineSize == string.Empty)
            {
                carBase.EngineSize = Sameknown.FirstOrDefault(s => s.Energy != null)?.EnginPower.ToString();
            }
            if (carBase.Trim == null)
            {
                carBase.Trim = Sameknown.FirstOrDefault(s => s.Trim != null)?.Trim;
            }
            if (carBase.ManufacturerSuggestedRetailPrice == null)
            {
                carBase.HermesMarketValue = await _carService.FindSameCarValue(carBase.Vin);
            }
            else
            {
                int age = DateTime.Now.Year - carBase.Year.Value;
                int value = int.Parse(carBase.ManufacturerSuggestedRetailPrice, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("us-US").NumberFormat);

                carBase.HermesMarketValue = await GetActualValue(value, age);
            }
            return carBase;
        }
        private async Task<int> GetActualValue (int value, int age)
        {
            if (age <= 0)
            {
                return value;
            }
            for (int i = 1; i < age; i++)
            {
                var taux = 0.0;
                switch (i)
                {
                    case 1:
                        {
                            taux = 0.2;
                            break;
                        }
                    case 2:
                        {
                            taux = 0.15;
                            break;

                        }
                    case 3:
                    case 4:
                        {
                            taux = 0.1;
                            break;
                        }
                    case 5:
                        {
                            taux = 0.07;
                            break;
                        }
                    default:
                        {
                            taux = 0.05;
                            break;
                        }
                }

                value = (int)(value * (1 - taux));
            }
            return value;
        }

    }
}

