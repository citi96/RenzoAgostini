using System.Net.Http.Json;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class CheckoutClient(HttpClient http) : ICheckoutClient
    {
        public async Task<StripeSessionDto> CreateCheckoutSessionAsync(CheckoutDto dto)
        {
            var res = await http.PostAsJsonAsync("api/orders/create-session", dto);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new InvalidOperationException(err != null && err.TryGetValue("Error", out var e) ? e : "Errore creazione sessione Stripe.");
            }
            return (await res.Content.ReadFromJsonAsync<StripeSessionDto>())!;
        }

        public async Task ConfirmPaymentAsync(string sessionId)
        {
            var res = await http.PostAsJsonAsync("api/payment/confirm", sessionId);
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException("Impossibile confermare il pagamento.");
        }
    }
}
