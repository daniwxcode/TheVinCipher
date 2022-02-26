using Domaine.Entities;

using HermesEyes.com.Abstracts;

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
            Message = "Impossible de trouver une côte pour cette voiture nos développeurs vous reviendrons";
        }
        public MarketValueResponse (string message)
        {
            Success = false;
            Data = null;
            Message = message;
        }
    }
}
