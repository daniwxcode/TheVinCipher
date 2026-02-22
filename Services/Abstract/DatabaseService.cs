using Infrastructure.Contexts;

namespace Services.Abstract
{
    public abstract class DatabaseService
    {
        protected VinCipherContext _dbContext;
        public DatabaseService (VinCipherContext vinCipherContext)
        {
            _dbContext = vinCipherContext;
        }
    }
}
