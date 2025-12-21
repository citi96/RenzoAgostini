using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface IBiographyRepository : IRepository<Biography>
    {
        // Add specific methods here if needed, e.g.
        Task<Biography?> GetSingletonAsync();
    }
}
