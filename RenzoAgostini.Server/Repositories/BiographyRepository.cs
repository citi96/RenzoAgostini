using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class BiographyRepository(RenzoAgostiniDbContext context) : RepositoryBase<Biography>(context), IBiographyRepository
    {
        protected override IQueryable<Biography> IncludeRelated(IQueryable<Biography> query) => query;

        public async Task<Biography?> GetSingletonAsync()
        {
            return await _dbSet.FirstOrDefaultAsync();
        }
    }
}
