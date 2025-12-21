using System.Net.Http.Json;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class ShippingClient(HttpClient http) : IShippingClient
    {
        public async Task<IReadOnlyList<ShippingOptionDto>> GetOptionsAsync(string country)
        {
            var res = await http.GetFromJsonAsync<IReadOnlyList<ShippingOptionDto>>($"api/shipping/options?country={Uri.EscapeDataString(country)}");
            return res ?? Array.Empty<ShippingOptionDto>();
        }

        public async Task<IReadOnlyList<ShippingOptionDto>> GetAllAsync()
        {
            var result = await http.GetFromJsonAsync<IReadOnlyList<ShippingOptionDto>>("api/shipping");
            return result ?? Array.Empty<ShippingOptionDto>();
        }

        public async Task<ShippingOptionDto> CreateAsync(CreateShippingOptionDto dto)
        {
            var response = await http.PostAsJsonAsync("api/shipping", dto);
            await EnsureSuccessAsync(response, "Impossibile creare la modalità di spedizione.");
            return (await response.Content.ReadFromJsonAsync<ShippingOptionDto>())!;
        }

        public async Task<ShippingOptionDto> UpdateAsync(int id, UpdateShippingOptionDto dto)
        {
            var response = await http.PutAsJsonAsync($"api/shipping/{id}", dto);
            await EnsureSuccessAsync(response, "Impossibile aggiornare la modalità di spedizione.");
            return (await response.Content.ReadFromJsonAsync<ShippingOptionDto>())!;
        }

        public async Task DeleteAsync(int id)
        {
            var response = await http.DeleteAsync($"api/shipping/{id}");
            await EnsureSuccessAsync(response, "Impossibile eliminare la modalità di spedizione.");
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string? errorMessage = null;
            try
            {
                var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (payload != null && payload.TryGetValue("Error", out var message) && !string.IsNullOrWhiteSpace(message))
                {
                    errorMessage = message;
                }
            }
            catch
            {
                // Ignora errori di deserializzazione
            }

            throw new InvalidOperationException(errorMessage ?? fallbackMessage);
        }
    }
}
