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
                var mycar = _dbContext.Cars.Where(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11)).ToList().Max(t=>t.MarketValue);
                //Car bases valides
                var carbases = _dbContext.CarsBases.Where(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11) || c.Vin.Substring(3, 6) == vin.Substring(3, 6)).ToList();
                carbase = carbases.FirstOrDefault(c => c.HermesMarketValue > 800000);

                // Si Absent
                if (carbase != null)
                {
                    carbase.Vin = vin;
                    if (carbase.HermesMarketValue != 0)
                    {
                        if (carbase.CreatedOn.Year <= DateTime.Today.Year + 1)
                        {
                            return carbase;
                        }
                    }
                }

                else
                {
                    carbase = await ManageOurVin(vin);
                    
                    //try
                    //{
                    //    // Recherche en Ligne
                    //    carbase = await _api.GetResult(vin);
                    //}catch (Exception ex)
                    //{
                    //    // Sinon Hermes

                    //}

                }
            }
            else
            {
                // Hermes
                carbase = await ManageOldVin(vin);
            }
            carbase = await OptimizeCarInfo(carbase);
            try
            {
                if (!_dbContext.CarsBases.Any(c => c.Vin == carbase.Vin))
                {
                    _dbContext.CarsBases.AddAsync(carbase);
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                return new CarBase();
            }

            return carbase;
        }

        private async Task<CarBase> OptimizeCarInfo (CarBase carBase)
        {
            var vin = carBase.Vin;
            var Sameknown = _dbContext.Cars.Where(c => c.MarketValue != 0 && (c.Vin.Substring(0, 11) == vin.Substring(0, 11) || c.Vin.Substring(3, 6) == vin.Substring(3, 6)));
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
            int age = DateTime.Now.Year - carBase.Year.Value;
            if (carBase.ManufacturerSuggestedRetailPrice == null)
            {
                carBase.HermesMarketValue = await _carService.FindSameCarValue(carBase.Vin);
            }
            else
            {

                carBase.HermesMarketValue = int.Parse(carBase.ManufacturerSuggestedRetailPrice, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("us-US").NumberFormat);


            }
            carBase.HermesMarketValue = await GetActualValue(carBase.HermesMarketValue, age);
            return carBase;
        }
        private async Task<CarBase> ManageOldVin (string vin)
        {

            var car = _dbContext.Cars.Where(c => c.Vin.Length <= 17 && c.Vin.Substring(0, 5) == vin.Substring(0, 5) && c.MarketValue != 0).FirstOrDefault();
            if (car == null)
                return null;
            var carBase = new CarBase()
            {
                Vin = vin,
                Doors = car.Doors,
                Model = car.Model,
                Make = car.Make,
                Category = car.Category + car.Trim,
                Transmission = car.Transmission,
                EngineCylinders = car.EngineCylender.ToString(),
                EngineSize = car.EnginPower.ToString(),
                Size = car.Size,
                StandardSeating = car.Seats,
                FuelType = car.Energy,
                Trim = car.Trim,
                Style = car.Type,
                HermesMarketValue = car.MarketValue.Value

            };
            if (carBase.Year != 0)
            {
                int age = DateTime.Now.Year - car.Year;
                int value = (int)(car.MarketValue * 1.1);
                carBase.HermesMarketValue = await GetActualValue(value, age);
            }


            return carBase;
        }
        private async Task<CarBase> ManageOurVin (string vin)
        {

            var car = _dbContext.Cars.Where(c => c.Vin.Length == 17 && c.Vin.Substring(0, 11) == vin.Substring(0, 11) && c.MarketValue != 0).OrderBy(t => t.MarketValue).Max();
            if (car == null)
                return null;
            var carBase = new CarBase()
            {
                Vin = vin,
                Doors = car.Doors,
                Model = car.Model,
                Make = car.Make,
                Category = car.Category + car.Trim,
                Transmission = car.Transmission,
                EngineCylinders = car.EngineCylender.ToString(),
                EngineSize = car.EnginPower.ToString(),
                Size = car.Size,
                StandardSeating = car.Seats,
                FuelType = car.Energy,
                Trim = car.Trim,
                Style = car.Type,
                HermesMarketValue = car.MarketValue.Value

            };
            if (carBase.Year != 0)
            {
                int age = DateTime.Now.Year - car.Year;
                int value = (int)(car.MarketValue * 1.1);
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
            age -= 5;
            for (int i = 1; i < age || i < 30; i++)
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

