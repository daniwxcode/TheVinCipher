using Domaine.Entities;

namespace Services.Interfaces
{
    public interface ICarService
    {
        public Task<int> FindSameCarValue (string carVin);

    }
}
