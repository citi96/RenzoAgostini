using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface ICustomOrderRepository : IRepository<CustomOrder>
    {
        Task<CustomOrder?> GetByAccessCodeAsync(string accessCode);
        Task<CustomOrder?> GetByAccessCodeAndEmailAsync(string accessCode, string email);
    }
}
