using Domaine.Interfaces;

using System.ComponentModel.DataAnnotations;

namespace Domaine.Abstracts
{
    public abstract class BaseEntity : IAuditable
    {
        [Key]
        public long ID { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
