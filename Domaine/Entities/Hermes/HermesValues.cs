using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domaine.Entities.Hermes
{
    public class HermesValues
    {
        [Key]
        public int Id { get; set; }
        public string? Vin { get; set; }
        public int HermesValue { get; set; }
        public DateTime valueDate { get; set; }= DateTime.Now;

    }
}
