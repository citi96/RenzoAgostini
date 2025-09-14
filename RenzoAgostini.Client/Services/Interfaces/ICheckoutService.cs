using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<StripeSessionDto> CreateCheckoutSessionAsync(CheckoutDto dto);
        Task ConfirmPaymentAsync(string sessionId);
    }
}
