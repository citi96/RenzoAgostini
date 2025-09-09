using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class CustomOrderRepository(RenzoAgostiniDbContext context) : RepositoryBase<CustomOrder>(context), ICustomOrderRepository
    {
        protected override IQueryable<CustomOrder> IncludeRelated(IQueryable<CustomOrder> query) =>
            query.Include(co => co.Painting)
                 .ThenInclude(p => p!.Images);

        public async Task<CustomOrder?> GetByAccessCodeAsync(string accessCode)
        {
            return await context.CustomOrders
                .Include(co => co.Painting)
                .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(co => co.AccessCode == accessCode);
        }

        public async Task<CustomOrder?> GetByAccessCodeAndEmailAsync(string accessCode, string email)
        {
            return await context.CustomOrders
                .Include(co => co.Painting)
                .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(co => co.AccessCode == accessCode && co.CustomerEmail == email);
        }
    }
}
