using Microsoft.Extensions.Caching.Memory;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services
{
    public class CachedPaintingService(
        IPaintingService innerService,
        IMemoryCache cache,
        ILogger<CachedPaintingService> logger) : IPaintingService
    {
        private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(30);

        public async Task<IEnumerable<PaintingDto>> GetAllPaintingsAsync()
        {
            const string cacheKey = "all_paintings";

            9if (cache.TryGetValue(cacheKey, out IEnumerable<PaintingDto>? cachedPaintings))
            {
                logger.LogInformation("Retrieved paintings from cache");
                return cachedPaintings!;
            }

            var paintings = await innerService.GetAllPaintingsAsync();

            cache.Set(cacheKey, paintings, _defaultCacheExpiration);
            logger.LogInformation("Cached {Count} paintings", paintings.Count());

            return paintings;
        }

        public async Task<PaintingDto?> GetPaintingByIdAsync(int id)
        {
            var cacheKey = $"painting_id_{id}";

            if (cache.TryGetValue(cacheKey, out PaintingDto? cachedPainting))
            {
                return cachedPainting;
            }

            var painting = await innerService.GetPaintingByIdAsync(id);

            if (painting != null)
            {
                cache.Set(cacheKey, painting, _defaultCacheExpiration);
            }

            return painting;
        }

        public async Task<PaintingDto?> GetPaintingBySlugAsync(string slug)
        {
            var cacheKey = $"painting_slug_{slug}";

            if (cache.TryGetValue(cacheKey, out PaintingDto? cachedPainting))
            {
                return cachedPainting;
            }

            var painting = await innerService.GetPaintingBySlugAsync(slug);

            if (painting != null)
            {
                cache.Set(cacheKey, painting, _defaultCacheExpiration);
            }

            return painting;
        }

        public async Task<PaintingDto> CreatePaintingAsync(CreatePaintingDto painting)
        {
            var result = await innerService.CreatePaintingAsync(painting);

            // Invalida cache
            cache.Remove("all_paintings");
            cache.Remove("paintings_for_sale");

            return result;
        }

        public async Task<PaintingDto> UpdatePaintingAsync(int id, CreatePaintingDto painting)
        {
            var result = await innerService.UpdatePaintingAsync(id, painting);

            // Invalida cache specifiche
            cache.Remove($"painting_id_{id}");
            cache.Remove("all_paintings");
            cache.Remove("paintings_for_sale");

            return result;
        }

        public async Task DeletePaintingAsync(int id)
        {
            await innerService.DeletePaintingAsync(id);

            // Invalida cache
            cache.Remove($"painting_id_{id}");
            cache.Remove("all_paintings");
            cache.Remove("paintings_for_sale");
        }

        public async Task<IEnumerable<PaintingDto>> GetPaintingsForSaleAsync()
        {
            const string cacheKey = "paintings_for_sale";

            if (cache.TryGetValue(cacheKey, out IEnumerable<PaintingDto>? cachedPaintings))
            {
                return cachedPaintings!;
            }

            var paintings = await innerService.GetPaintingsForSaleAsync();

            cache.Set(cacheKey, paintings, _defaultCacheExpiration);

            return paintings;
        }
    }
}
