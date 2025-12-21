using System.Net.Http.Json;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class BiographyService(HttpClient httpClient) : IBiographyService
    {
        public async Task<BiographyDto> GetAsync()
        {
            return await httpClient.GetFromJsonAsync<BiographyDto>("api/Biography")
                   ?? new BiographyDto("", null);
        }

        public async Task UpdateAsync(BiographyDto dto)
        {
            await httpClient.PutAsJsonAsync("api/Biography", dto);
        }
    }
}
