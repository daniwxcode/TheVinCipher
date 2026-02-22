using Domaine.Entities;
using Domaine.Entities.VinCipher;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class VinCipherContext : DbContext
    {      
        public DbSet<Requests> Requests { get; set; }
        public DbSet<CarBase> CarsBases { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<VinCipherCar> VinCipherCars { get; set; }
        public DbSet<VinCipherValues>  VinCipherValues { get; set; }
        public VinCipherContext (DbContextOptions<VinCipherContext> options)
            : base(options)
        {

        }
    }
}
