using Domaine.Abstracts;
using Domaine.Interfaces;

using System.ComponentModel.DataAnnotations.Schema;

namespace Domaine.Entities
{
    public class Requests : BaseEntity, ISearchable
    {
        public string Vin { get; set; }

        [ForeignKey("CarID")]
        public virtual Car? Car { get; set; }
        public long? CarID { get; set; }

        public bool IsManaged { get; set; } = false;

    }
}
