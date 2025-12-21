using RenzoAgostini.Shared.Common;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services.Interfaces
{
    public interface IOrderService : Shared.Contracts.IOrderService
    {
        /// <summary>Crea un nuovo ordine e avvia una sessione di pagamento Stripe.</summary>
        Task<Result<string>> CreateOrderAndStartPaymentAsync(CheckoutDto checkout);
        /// <summary>Conferma il pagamento completato aggiornando l'ordine e i quadri.</summary>
        Task<Result<OrderDto>> ConfirmOrderPaymentAsync(string stripeSessionId);
    }
}
