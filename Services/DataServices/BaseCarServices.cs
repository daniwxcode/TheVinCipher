using Domaine.Entities;

using Infrastructure.Contexts;

using Services.Interfaces;

namespace Services.DataServices
{
    public class BaseCarServices : ICarService
    {
        private readonly HermesContext _Repository;
        public BaseCarServices (HermesContext hermesContext)
        {
            _Repository = hermesContext;
        }
        public async Task<int> FindSameCarValue (string carVin)
        {
            
            List<(int Effectif, double ValeurMoyenne)> distribution = new List<(int, double)>();
            var car = new Car();
            var cars = new List<Car>();
            int taille = carVin.Length;

            int limit = 0;
            switch (taille)
            {
                case 17:
                    {
                        limit = 11;
                        break;
                    }
                default:
                    {
                        limit = 5;
                        break;
                    }
            }
            var ourValue = _Repository.CarsBases.FirstOrDefault(c => c.Vin.Substring(0, limit) == carVin.Substring(0, limit)).HermesMarketValue;
            if(ourValue != null && ourValue!=0)
                return ourValue;
            car = _Repository.Cars.Where(c => c.Vin.Substring(0, limit) == carVin.Substring(0, limit)).OrderByDescending(m=>m.MarketValue).FirstOrDefault();
            if (car == null)
            {
                return 0;
            }
            car = null;
            Car cartemoin = new Car();
            for (int i = taille; i >= limit; i--)
            {
                var taux = 0.0;
                var chaine = carVin.Substring(0, i);
                cars = _Repository.Cars.Where(c => c.Vin.Substring(0, i) == chaine).ToList();

                if (cars != null && cars.Any())
                {

                    switch (DateTime.Now.Year - cars.Min(c => c.Year))
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

                    if (car == null)
                    {
                        car = cars.First();
                    }
                    cartemoin = cars.Where(c=>c.MarketValue == cars.Max(cv=>cv.MarketValue)).FirstOrDefault();
                    var nb = cars.Count;
                    var val = cars.Average(c => c.MarketValue).Value * (1 - taux);
                    distribution.Add((nb, val));
                }
            }

            var proche = _Repository.Cars.Where(c => c.Vin.Substring(0, limit) == carVin.Substring(0, limit)).OrderBy(t => t.ValueDate).LastOrDefault().MarketValue;

            var diff = Math.Abs(proche.Value - cartemoin.MarketValue.Value);
            var somme = distribution.Sum(i => (i.ValeurMoyenne * i.Effectif));
            var effectif = distribution.Sum(c => c.Effectif);
            var groupe = distribution.Count();


            int age = DateTime.Now.Year - car.Year;
            int ajout = 0;
            if (age < 5)
            {
                var valeurCadre = (int)(somme / effectif);
                if (car.MarketValue < valeurCadre)
                {
                    ajout = 0;
                }
                else
                {
                    ajout = diff;
                }

            }
            else
            {
                if (age > 20)
                {
                    Random rnd = new Random();
                    int taux = rnd.Next(1, 13);
                    ajout = car.MarketValue.Value * (taux / 100);
                }
            }

            car.Vin = carVin;
            car.ValueDate = DateTime.Now;
            car.MarketValue = (int)(somme / effectif) + new Random().Next(0, ajout);
            
            if (DateTime.Today.Year - car.Year < 10 &&car.MarketValue<3_000_000 )
            {
                return _Repository.Cars.FirstOrDefault(c => c.Year == car.Year && c.MarketValue > 3_000_000).MarketValue??3_000254+new Random().Next(20000,500_000);
              
            }

            return car.MarketValue??0;
          //  return new Car();
        }
    }
}
