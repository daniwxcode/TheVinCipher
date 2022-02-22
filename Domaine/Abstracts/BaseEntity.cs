using Domaine.Interfaces;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domaine.Abstracts
{
    public abstract class BaseEntity : IAuditable
    {
        [Key]
        public long ID { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

    }
}
