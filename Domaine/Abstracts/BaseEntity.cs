using Domaine.Interfaces;

using System.ComponentModel.DataAnnotations;

namespace Domaine.Abstracts
{
    public abstract class BaseEntity : IAuditable
    {
       
        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
