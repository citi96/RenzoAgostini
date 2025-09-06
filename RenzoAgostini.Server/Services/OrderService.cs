using System.Net;
using Microsoft.Extensions.Options;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Exceptions;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Common;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;
using Stripe;
using Stripe.Checkout;

namespace RenzoAgostini.Server.Services
{
    public class OrderService(IOrderRepository orderRepo, IPaintingRepository paintingRepo,
                         IOptions<StripeOptions> stripeOptions, ILogger<OrderService> logger) : IOrderService
    {
        private readonly StripeOptions _stripeOptions = stripeOptions.Value;

        public async Task<Result<string>> CreateOrderAndStartPaymentAsync(CheckoutDto checkout)
        {
            // 1. Validazione input
            if (!checkout.TermsAccepted)
            {
                return Result<string>.Failure("Devi accettare le condizioni di vendita prima di procedere all'acquisto.");
            }
            if (checkout.PaintingIds == null || checkout.PaintingIds.Count == 0)
            {
                return Result<string>.Failure("Nessun articolo presente nel carrello.");
            }

            // 2. Recupera i quadri dal DB e verifica disponibilità
            var paintings = new List<Painting>();
            decimal totalAmount = 0m;
            foreach (int paintingId in checkout.PaintingIds)
            {
                var painting = await paintingRepo.GetByIdAsync(paintingId) ??
                    throw new ApiException(HttpStatusCode.NotFound, $"Il quadro con ID {paintingId} non esiste.");

                if (!painting.IsForSale)
                    throw new ApiException(HttpStatusCode.NotFound, $"Il quadro '{painting.Title}' non è più disponibile per la vendita.");
                if (painting.Price == null)
                    throw new ApiException(HttpStatusCode.BadRequest, $"Il quadro '{painting.Title}' non ha un prezzo definito.");

                paintings.Add(painting);
                totalAmount += painting.Price.Value;
            }

            // 3. Crea l'oggetto Order (Pending) con gli OrderItem
            var order = new Order
            {
                CustomerFirstName = checkout.CustomerFirstName,
                CustomerLastName = checkout.CustomerLastName,
                CustomerEmail = checkout.CustomerEmail,
                AddressLine = checkout.AddressLine,
                City = checkout.City,
                PostalCode = checkout.PostalCode,
                Country = checkout.Country,
                TermsAccepted = checkout.TermsAccepted,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                Items = []
            };

            foreach (var painting in paintings)
            {
                order.Items.Add(new OrderItem
                {
                    PaintingId = painting.Id,
                    PaintingTitle = painting.Title,
                    Price = painting.Price!.Value
                });
            }

            try
            {
                // 4. Prepara la Sessione di Checkout Stripe
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = ["card", "klarna"],
                    LineItems = [],
                    Mode = "payment",
                    SuccessUrl = _stripeOptions.SuccessUrl,
                    CancelUrl = _stripeOptions.CancelUrl,
                    BillingAddressCollection = "required"  // chiedi l'indirizzo di fatturazione
                };

                // Aggiunge ogni quadro come line item nella sessione Stripe
                foreach (var painting in paintings)
                {
                    var lineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = Convert.ToInt64(painting.Price!.Value * 100), // centesimi
                            Currency = "eur", // valuta fissa EUR (configurabile se necessario)
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = painting.Title
                            }
                        },
                        Quantity = 1
                    };
                    options.LineItems.Add(lineItem);
                }

                var sessionService = new SessionService();
                Session session = sessionService.Create(options);

                // 5. Salva l'ordine nel DB con la StripeSessionId
                order.StripeSessionId = session.Id;
                await orderRepo.AddAsync(order);

                logger.LogInformation("Creato ordine {OrderId} con sessione Stripe {SessionId}", order.Id, session.Id);
                return Result<string>.Success(session.Id);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Errore durante la creazione della sessione di pagamento Stripe");
                throw new ApiException(HttpStatusCode.InternalServerError, "Errore nella comunicazione col gateway di pagamento. Riprova più tardi.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante la creazione dell'ordine");
                throw new ApiException(HttpStatusCode.InternalServerError, "Errore interno durante la creazione dell'ordine.");
            }
        }

        public async Task<Result<Order>> ConfirmOrderPaymentAsync(string stripeSessionId)
        {
            // 1. Trova l'ordine corrispondente alla sessione
            var order = await orderRepo.GetByStripeSessionAsync(stripeSessionId) ??
                throw new ApiException(HttpStatusCode.NotFound, "Ordine non trovato per la sessione specificata.");

            if (order.Status == OrderStatus.Paid)
                return Result<Order>.Success(order);

            try
            {
                // 2. Recupera i dettagli della sessione da Stripe per verificare pagamento
                var sessionService = new SessionService();
                Session session = sessionService.Get(stripeSessionId);
                if (session.Status != "complete" || session.PaymentStatus != "paid")
                {
                    // Pagamento non completato (l'utente potrebbe aver annullato)
                    order.Status = OrderStatus.Cancelled;
                    await orderRepo.UpdateAsync(order);

                    logger.LogWarning("Pagamento non completato per ordine {OrderId}", order.Id);
                    throw new ApiException(HttpStatusCode.InternalServerError, "Pagamento non completato o annullato.");
                }

                // 3. Pagamento riuscito - aggiorna stato ordine
                order.Status = OrderStatus.Paid;
                await orderRepo.UpdateAsync(order);

                // 4. Aggiorna lo stato di ogni quadro come venduto (IsForSale = false)
                foreach (var item in order.Items)
                {
                    var painting = await paintingRepo.GetByIdAsync(item.PaintingId);
                    if (painting != null)
                    {
                        painting.IsForSale = false;
                        await paintingRepo.UpdateAsync(painting);
                    }
                }
                logger.LogInformation("Ordine {OrderId} confermato come PAGATO.", order.Id);
                return Result<Order>.Success(order);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Errore nel recupero della sessione Stripe {SessionId}", stripeSessionId);
                throw new ApiException(HttpStatusCode.InternalServerError, "Impossibile verificare il pagamento con Stripe. Riprova più tardi.");
            }
        }
    }
}
