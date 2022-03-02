using Domaine.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class HermesContext : DbContext
    {      
        public DbSet<Requests> Requests { get; set; }
        public DbSet<CarBase> CarsBases { get; set; }
        public DbSet<Car> Cars { get; set; }
        public HermesContext (DbContextOptions<HermesContext> options)
            : base(options)
        {

        }
    }
}
