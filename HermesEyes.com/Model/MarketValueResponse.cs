using Domaine.Entities;

using HermesEyes.com.Abstracts;

namespace HermesEyes.com.Model
{
    public class MarketValueResponse : HermesResponse<CarDecode>
    {
        public MarketValueResponse (CarDecode car)
        {
            Success = car.HermesMarketValue!=0;            
            Data = car;
            Message = car.HermesMarketValue!=0?"Evaluation effectuée avec succès": "Impossible de trouver une côte pour cette voiture nos développeurs vous reviendrons";
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
