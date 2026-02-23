using Domaine.Entities;

using Infrastructure.APIs.Abstracts;
using Infrastructure.Contexts;

using Services.Interfaces;

using System.Globalization;

namespace Services.DataServices
{
    public class DecodedCarBaseService : IHttpConsumtionServices
    {
        private readonly VinCipherContext _dbContext;
        private readonly BaseApiProviderClient<CarBase> _api;
        private readonly BaseApiProvider _baseApiProvider;
        private readonly ICarService _carService;
        public DecodedCarBaseService (VinCipherContext context, HttpClient httpClient, BaseApiProvider apiProvider, BaseApiProviderClient<CarBase> client, ICarService carService)
        {
            _dbContext = context;
            _baseApiProvider = apiProvider;
            _carService = carService;
            _api = client;
        }

        public async Task<CarBase> FindCar (string vin)
        {
            if (vin.IsValid())
            {
                CarBase carbase = null;
                try
                {
                    // Recherche en Ligne
                    carbase = await _api.GetResult(vin);
                    if (carbase != null)
                    {
                        carbase.Year = vin.GetModelYear();
                        carbase.Vin = vin;
                        carbase.MarketValue = int.Parse(carbase?.ManufacturerSuggestedRetailPrice, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("us-US").NumberFormat) * 600;
                        int age = DateTime.Today.Year - carbase.Year.Value;
                        if (age > 4)
                        {
                            carbase.MarketValue = await GetActualValue(carbase.MarketValue, age);
                        }

                        return carbase;
                    }

                }
                catch (Exception e)
                {
                   // return null;
                }
            }
            return null;
        }
        #region ignore
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
            int age = DateTime.Now.Year - carBase?.Year ?? 20;
            if (carBase.ManufacturerSuggestedRetailPrice == null)
            {
                int val = _dbContext.CarsBases.Where(c => c.Vin.Substring(0, 11) == carBase.Vin.Substring(0, 11)).Max(m => m.MarketValue);
                var val2 = _dbContext.CarsBases.Where(c => c.Vin.Substring(0, 11) == carBase.Vin.Substring(0, 11)).Average(m => m.MarketValue);
                if (val == 0)
                {
                    carBase.MarketValue = (int)val2;
                }
                else
                {
                    carBase.MarketValue = val;
                }

            }
            else
            {

                carBase.MarketValue = int.Parse(carBase.ManufacturerSuggestedRetailPrice, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("us-US").NumberFormat) * 600;

            }
            carBase.MarketValue = await GetActualValue(carBase.MarketValue, age);
            try
            {
                _dbContext.CarsBases.Update(carBase);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
            return carBase;
        }

        private async Task<CarBase> ManageOldVin (string vin)
        {

            var car = _dbContext.Cars.Where(c => c.Vin.Length <= 17 && c.Vin.Substring(0, 5) == vin.Substring(0, 5) && c.MarketValue != 0).FirstOrDefault();
            if (car == null)
            {
                return null;
            }

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
                MarketValue = car.MarketValue.Value

            };
            if (carBase.Year != 0)
            {
                int age = DateTime.Now.Year - car.Year.Value;
                int value = (int)(car.MarketValue * 1.1);
                carBase.MarketValue = await GetActualValue(value, age);
            }


            return carBase;
        }
        private async Task<CarBase> ManageOurVin (string vin)
        {

            var car = _dbContext.Cars.Where(c => c.Vin.Length == 17 && c.Vin.Substring(0, 11) == vin.Substring(0, 11) && c.MarketValue != 0).OrderBy(t => t.MarketValue).Max();
            if (car == null)
            {
                return null;
            }

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
                MarketValue = car.MarketValue.Value

            };
            if (carBase.Year != 0)
            {
                int age = DateTime.Now.Year - car.Year.Value;
                int value = (int)(car.MarketValue * 1.1);
                carBase.MarketValue = await GetActualValue(value, age);
            }


            return carBase;
        }
        #endregion
        private async Task<int> GetActualValue (int value, int age)
        {
           
          age -= 5;
            for (int i = 1; i < age && i < 30; i++)
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
            //if(age<=3)
            //{
            //    return value<8_000_000 ? _dbContext.CarsBases.FirstOrDefault(c => c.HermesMarketValue>10_000_000).HermesMarketValue : value;
            //}
            //if(age<=5)
            //{
            //    return value<4_000_000 ? _dbContext.CarsBases.FirstOrDefault(c => c.HermesMarketValue>5_000_000).HermesMarketValue : value;
            //}
            //if(age<=15)
            //{
            //    return value<1_000_000 ? _dbContext.CarsBases.FirstOrDefault(c => c.HermesMarketValue>1_000_000).HermesMarketValue : value;
            //}
            return value;
        }

    }
}

