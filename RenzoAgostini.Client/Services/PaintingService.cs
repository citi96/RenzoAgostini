using System.Net.Http.Json;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class PaintingService(HttpClient client) : IPaintingService
    {
            private const string BasePath = "api/paintings";

            public async Task<IEnumerable<PaintingDto>> GetAllPaintingsAsync()
            {
                var res = await client.GetFromJsonAsync<IEnumerable<PaintingDto>>(BasePath);
                return res ?? Enumerable.Empty<PaintingDto>();
            }

            public async Task<PaintingDto?> GetPaintingByIdAsync(int id)
            {
                return await client.GetFromJsonAsync<PaintingDto>($"{BasePath}/{id}");
            }

            public async Task<PaintingDto?> GetPaintingBySlugAsync(string slug)
            {
                return await client.GetFromJsonAsync<PaintingDto>($"{BasePath}/slug/{Uri.EscapeDataString(slug)}");
            }

            public async Task<PaintingDto> CreatePaintingAsync(CreatePaintingDto painting)
            {
                var res = await client.PostAsJsonAsync(BasePath, painting);
                res.EnsureSuccessStatusCode();
                return (await res.Content.ReadFromJsonAsync<PaintingDto>())!;
            }

            public async Task<PaintingDto> UpdatePaintingAsync(int id, CreatePaintingDto painting)
            {
                var res = await client.PutAsJsonAsync($"{BasePath}/{id}", painting);
                res.EnsureSuccessStatusCode();
                return (await res.Content.ReadFromJsonAsync<PaintingDto>())!;
            }

            public async Task DeletePaintingAsync(int id)
            {
                var res = await client.DeleteAsync($"{BasePath}/{id}");
                res.EnsureSuccessStatusCode();
            }

            public async Task<IEnumerable<PaintingDto>> GetPaintingsForSaleAsync()
            {
                var res = await client.GetFromJsonAsync<IEnumerable<PaintingDto>>($"{BasePath}/for-sale");
                return res ?? Enumerable.Empty<PaintingDto>();
            }
    }
}
