using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByStripeSessionAsync(string sessionId);
    }
}
