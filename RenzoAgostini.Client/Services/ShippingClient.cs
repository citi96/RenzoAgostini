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
    }
}
