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
    public class VinToSearchServices : ICrudServices
    {
        private readonly HermesContext _repository;
        public VinToSearchServices (HermesContext hermes)
        {
            _repository = hermes;
        }
        public async void Ajouter (string element)
        {
            var item = new Requests()
            {
                Vin = element
            };
            try
            {
                await _repository.Set<Requests>().AddAsync(item);
                await _repository.SaveChangesAsync();
            }catch (Exception ex)
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
