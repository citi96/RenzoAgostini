using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Shared.Contracts
{
    public interface IPaintingService
    {
        Task<IEnumerable<PaintingDto>> GetAllPaintingsAsync();
        Task<PaintingDto?> GetPaintingByIdAsync(int id);
        Task<PaintingDto?> GetPaintingBySlugAsync(string slug);
        Task<PaintingDto> CreatePaintingAsync(CreatePaintingDto painting);
        Task<PaintingDto> UpdatePaintingAsync(int id, CreatePaintingDto painting);
        Task DeletePaintingAsync(int id);
        Task<IEnumerable<PaintingDto>> GetPaintingsForSaleAsync();
    }
}
