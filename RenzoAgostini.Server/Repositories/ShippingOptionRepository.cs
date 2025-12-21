using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class ShippingOptionRepository(RenzoAgostiniDbContext context)
        : RepositoryBase<ShippingOption>(context), IShippingOptionRepository
    {
        protected override IQueryable<ShippingOption> IncludeRelated(IQueryable<ShippingOption> query)
            => query;

        public async Task<IReadOnlyList<ShippingOption>> GetActiveAsync()
        {
            var activeOptions = await _dbSet
                .Where(option => option.IsActive)
                .ToListAsync();

            return activeOptions
                .OrderBy(option => option.IsPickup ? 0 : 1)
                .ThenBy(option => option.Cost)
                .ThenBy(option => option.Name)
                .ToList();
        }

        public Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.AsQueryable().Where(option => option.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(option => option.Id != excludeId.Value);
            }

            return query.AnyAsync();
        }
    }
}
