using Domaine.Entities;

namespace Services.Interfaces
{
    public interface IHttpConsumtionServices
    {
        public Task<CarBase> FindCar (string vin);
    }
}
