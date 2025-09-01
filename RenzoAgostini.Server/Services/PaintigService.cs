using RenzoAgostini.Server.Mappings;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services
{
    public class PaintingService(IPaintingRepository repository, ILogger<PaintingService> logger) : IPaintingService
    {
        public async Task<IEnumerable<PaintingDto>> GetAllPaintingsAsync()
        {
            try
            {
                var paintings = await repository.GetAllAsync();
                return paintings.Select(p => p.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all paintings");
                throw;
            }
        }

        public async Task<PaintingDto?> GetPaintingByIdAsync(int id)
        {
            try
            {
                var painting = await repository.GetByIdAsync(id);
                return painting?.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving painting with ID {PaintingId}", id);
                throw;
            }
        }

        public async Task<PaintingDto?> GetPaintingBySlugAsync(string slug)
        {
            try
            {
                var painting = await repository.GetBySlugAsync(slug);
                return painting?.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving painting with slug {Slug}", slug);
                throw;
            }
        }

        public async Task<PaintingDto> CreatePaintingAsync(CreatePaintingDto paintingDto)
        {
            try
            {
                var painting = paintingDto.ToEntity();
                var createdPainting = await repository.CreateAsync(painting);
                return createdPainting.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating painting {Title}", paintingDto.Title);
                throw;
            }
        }

        public async Task<PaintingDto> UpdatePaintingAsync(int id, CreatePaintingDto paintingDto)
        {
            try
            {
                var existingPainting = await repository.GetByIdAsync(id) ?? 
                    throw new ArgumentException($"Painting with ID {id} not found");

                var updatedPainting = paintingDto.ApplyTo(existingPainting);
                var result = await repository.UpdateAsync(updatedPainting);
                return result.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating painting with ID {PaintingId}", id);
                throw;
            }
        }

        public async Task DeletePaintingAsync(int id)
        {
            try
            {
                await repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting painting with ID {PaintingId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PaintingDto>> GetPaintingsForSaleAsync()
        {
            try
            {
                var paintings = await repository.GetAllAsync();
                return paintings.Where(p => p.IsForSale).Select(p => p.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving paintings for sale");
                throw;
            }
        }
    }
}
