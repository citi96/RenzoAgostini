using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class OrderRepository(RenzoAgostiniDbContext context) : RepositoryBase<Order>(context), IOrderRepository
    {
        protected override IQueryable<Order> IncludeRelated(IQueryable<Order> query) =>
            query.Include(o => o.Items)
                 .Include(o => o.ShippingOption);

        public async Task<Order?> GetByStripeSessionAsync(string sessionId)
        {
            return await context.Orders
                .Include(o => o.Items)
                .Include(o => o.ShippingOption)
                .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);
        }
    }
}
