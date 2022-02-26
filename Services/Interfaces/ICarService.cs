using Domaine.Entities;

namespace Services.Interfaces
{
    public interface ICarService
    {
        public Task<Car> FindSameCar (string carVin);

    }
}
