using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface IShippingOptionRepository : IRepository<ShippingOption>
    {
        Task<IReadOnlyList<ShippingOption>> GetActiveAsync();
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
}
