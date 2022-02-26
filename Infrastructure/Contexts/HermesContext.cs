using Domaine.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class HermesContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Requests> Requests { get; set; }
        public DbSet<CarBase> CarsBase { get; set; }

        public HermesContext (DbContextOptions<HermesContext> options)
            : base(options)
        {

        }
    }
}
