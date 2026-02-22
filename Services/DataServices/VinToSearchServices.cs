using Domaine.Entities;

using Infrastructure.Contexts;

using Services.Interfaces;

namespace Services.DataServices
{
    public class VinToSearchServices : ICrudServices
    {
        private readonly VinCipherContext _repository;
        public VinToSearchServices (VinCipherContext context)
        {
            _repository = context;
        }
        public async Task Ajouter (string element)
        {
            var item = new Requests()
            {
                Vin = element
            };
            try
            {
                await _repository.AddAsync(item);
                await _repository.SaveChangesAsync();
            }
            catch (Exception)
            {
                //Ignore
            }


        }

        public IEnumerable<string> ListeElements ()
        {
            return _repository.Requests.Where(i => !i.IsManaged).Select(c => c.Vin).ToList();

        }

        public string lienDeRecherche (string element)
        {
            throw new NotImplementedException();
        }
    }
}
