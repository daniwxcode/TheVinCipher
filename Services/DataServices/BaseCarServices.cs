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
        public BaseCarServices (HermesContext hermesContext)
        {
            _Repository = hermesContext;
        }
        public async Task<Car> FindSameCar (string carVin)
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
            car = _Repository.Cars.Where(c => c.Vin.Substring(0, limit) == carVin.Substring(0,limit)).FirstOrDefault();
            if (car == null)
            {
                return null;
            }
            car = null;
            var cartemoin = new Car();
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
                    cartemoin = cars.Last();
                    var nb = cars.Count;
                    var val = cars.Average(c => c.MarketValue) * (1 - taux);
                    distribution.Add((nb, val));
                }
            }

            var proche = _Repository.Cars.Where(c => c.Vin.Substring(0, limit) == carVin.Substring(0,limit)).OrderBy(t => t.ValueDate).LastOrDefault().MarketValue;

            var diff = Math.Abs(proche - cartemoin.MarketValue);
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
                    ajout =diff;
                }

            }
            else
            {
                if (age > 20)
                {
                    Random rnd = new Random();
                    int taux = rnd.Next(1,13);
                    ajout =(int) car.MarketValue*(taux/100);
                }
            }


            car.Vin = carVin;
            car.ValueDate = DateTime.Now;
            car.MarketValue = (int)(somme / effectif)+new Random().Next(0,ajout);
            car.ID = 0;
            //try
            //{
            //    _Repository.Add(car);
            //    _Repository.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            return car;
        }
    }
}
