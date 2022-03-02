using Domaine.Abstracts;
using Domaine.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domaine.Entities
{
    public class Requests : BaseEntity, ISearchable
    {
        [Key]
        public string Vin { get; set; }

        public bool IsManaged { get; set; } = false;

    }
}
