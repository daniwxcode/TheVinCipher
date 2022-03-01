using Domaine.Abstracts;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domaine.Entities
{
    [Index(nameof(Vin), IsUnique = true)]
    [Table("CarsDecoding")]
    public class CarBase : Car
    {      
        public string? ManufacturerSuggestedRetailPrice { get; set; }
        public DateTime? MadeDeate { get; set; }

        public bool IsISOVin () => Vin.Length == 17;
        public bool IsFromUS () => Vin[3] == 9;
      
    }
}
