using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public interface IPaintingRepository
    {
        Task<IEnumerable<Painting>> GetAllAsync();
        Task<Painting?> GetByIdAsync(int id);
        Task<Painting?> GetBySlugAsync(string slug);
        Task<Painting> CreateAsync(Painting painting);
        Task<Painting> UpdateAsync(Painting painting);
        Task DeleteAsync(int id);
    }
}
