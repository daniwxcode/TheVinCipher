using Domaine.Entities;

using HermesEyes.com.Abstracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesEyes.com.Model
{
    public class MarketValueResponse : HermesResponse<Car>
    {
        public MarketValueResponse (Car car)
        {
            Success = true;
            Data = car;
            Message = "Evaluation effectuée avec succès";
        }
        public MarketValueResponse ()
        {
            Success = false;
            Data = null;
            Message = "Impossible de trouver une côte pour cette voiture nos développeurs vous reviendons";
        }
    }
}
