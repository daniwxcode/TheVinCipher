using Domaine.Entities;

using Infrastructure.Contexts;

using Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataServices
{
    public class BaseCarServices : ICarService
    {
        private readonly HermesContext _Repository;
        public BaseCarServices(HermesContext hermesContext)
        {
            _Repository = hermesContext;
        }
        public async Task<Car> FindSameCar (string carVin)
        {
            int taille = carVin.Length;
            var chaine = string.Empty;
            switch (taille)
            {
                case 17:
                    {
                        chaine = carVin.Substring(0, 11);

                        break;
                    }
                default:
                    {
                        chaine = carVin.Substring (0, 5);
                        break;
                    }
            }

            var car = _Repository.Cars.Where(c => c.Vin.Substring(0,chaine.Length)==chaine).FirstOrDefault();
            if(car == null)
            {
                return null;
            }
            car.Vin = carVin;
            car.ValueDate = DateTime.Now;
            return car;
        }
    }
}
