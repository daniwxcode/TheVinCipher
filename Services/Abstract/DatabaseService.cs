using Infrastructure.Contexts;

namespace Services.Abstract
{
    public abstract class DatabaseService
    {
        protected HermesContext _dbContext;
        public DatabaseService (HermesContext hermesContext)
        {
            _dbContext = hermesContext;
        }
    }
}
