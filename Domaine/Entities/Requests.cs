using Domaine.Abstracts;
using Domaine.Interfaces;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domaine.Entities
{
    public class Requests: BaseEntity, ISearchable
    {
        public string  Vin { get; set; }
               
        [ForeignKey("CarID")]
        public virtual Car? Car { get; set; }
        public long? CarID { get; set; }
        
        public bool IsManaged { get; set; } = false;

    }
}
