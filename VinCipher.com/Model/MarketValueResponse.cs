using Domaine.Entities;

using VinCipher.Abstracts;

namespace VinCipher.Model
{
    public class MarketValueResponse {
        public  bool Success { get; }
        public MarketValue Data { get; init; } = new MarketValue(0);
        public string Message { get; init; }

        public MarketValueResponse (MarketValue car)
        {
            Success = car.Value!=0;            
            Data = car;
            Message = car.Value!=0?"Evaluation effectuée avec succès": "Impossible de trouver une côte pour cette voiture nos développeurs vous reviendrons";
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
