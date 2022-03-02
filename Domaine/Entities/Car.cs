using Domaine.Abstracts;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domaine.Entities
{
    [Keyless]
    public class Car : BaseEntity
    {
        public string Vin { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int? EnginPower { get; set; }
        public string? Category { get; set; }
        public string? Transmission { get; set; }
        public string? Energy { get; set; }
        public double? EngineCylender { get; set; }
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string? Type { get; set; }
        public string? Trim { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public int? MarketValue { get; set; }
        public DateTime? ValueDate { get; set; }
        public DateTime? MadeDeate { get; set; }
        public override bool Equals (object? obj)
        {
            return obj is CarDecode car && GetHashCode() == obj.GetHashCode();
        }

        public Car ()
        {

        }
        
        public override int GetHashCode ()
        {
            int i = 11;
            if (Vin.Length < 17)
                i = 5;
            if (Vin.Length != 0)
                return HashCode.Combine(Vin.Substring(0, i));
            return 0;
        }
    }
}
