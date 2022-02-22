using Domaine.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICarService
    {
        public Task<Car> FindSameCar(string carVin);

    }
}
