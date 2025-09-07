using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface ICheckoutClient
    {
        Task<StripeSessionDto> CreateCheckoutSessionAsync(CheckoutDto dto);
        Task ConfirmPaymentAsync(string sessionId);
    }
}
