using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface IPaintingRepository : IRepository<Painting>
    {
        Task<Painting?> GetBySlugAsync(string slug);
    }
}
