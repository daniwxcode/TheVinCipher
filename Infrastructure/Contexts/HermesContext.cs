using Domaine.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Contexts
{
    public  class HermesContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Requests> Requests { get; set; }
              
        public HermesContext (DbContextOptions<HermesContext> options)
            : base(options)
        {
            
        }
    }
}
