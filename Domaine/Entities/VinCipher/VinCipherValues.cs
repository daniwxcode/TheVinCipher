using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domaine.Entities.VinCipher
{
    public class VinCipherValues
    {
        [Key]
        public int Id { get; set; }
        public string? Vin { get; set; }
        public int CipherValue { get; set; }
        public DateTime valueDate { get; set; }= DateTime.Now;

    }
}
