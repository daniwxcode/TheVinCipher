using Domaine.Abstracts;
using Domaine.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICrudServices
    {
        public void Ajouter (string element);
        public IEnumerable<string> ListeElements ();
        public string lienDeRecherche (string element);

    }

     
}
