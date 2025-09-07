using System.Net;
using Microsoft.Extensions.Options;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Emailing.Interfaces;
using RenzoAgostini.Server.Emailing.Models;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Exceptions;
using RenzoAgostini.Server.Mappings;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Common;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;
using Stripe;
using Stripe.Checkout;

namespace RenzoAgostini.Server.Services
{
    public class OrderService(IOrderRepository orderRepository, IPaintingRepository paintingRepository,
                         IOptions<StripeOptions> stripeOptions, ICustomEmailSender emailSender, IWebHostEnvironment env, ILogger<OrderService> logger) : IOrderService
    {
        private readonly StripeOptions _stripeOptions = stripeOptions.Value;

        public async Task<Result<string>> CreateOrderAndStartPaymentAsync(CheckoutDto checkout)
        {
            // 1. Validazione input
            if (!checkout.TermsAccepted)
                throw new ApiException(HttpStatusCode.BadRequest, "Devi accettare le condizioni di vendita prima di procedere all'acquisto.");
            if (checkout.PaintingIds == null || checkout.PaintingIds.Count == 0)
                throw new ApiException(HttpStatusCode.BadRequest, "Nessun articolo presente nel carrello.");

            // 2. Recupera i quadri dal DB e verifica disponibilità
            var paintings = new List<Painting>();
            decimal totalAmount = 0m;
            foreach (int paintingId in checkout.PaintingIds)
            {
                var painting = await paintingRepository.GetByIdAsync(paintingId) ??
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
                await orderRepository.AddAsync(order);

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

        public async Task<Result<OrderDto>> ConfirmOrderPaymentAsync(string stripeSessionId)
        {
            // 1. Trova l'ordine corrispondente alla sessione
            var order = await orderRepository.GetByStripeSessionAsync(stripeSessionId) ??
                throw new ApiException(HttpStatusCode.NotFound, "Ordine non trovato per la sessione specificata.");

            if (order.Status == OrderStatus.Paid)
                return Result<OrderDto>.Success(order.ToOrderAdminDto());

            try
            {
                // 2. Recupera i dettagli della sessione da Stripe per verificare pagamento
                var sessionService = new SessionService();
                Session session = sessionService.Get(stripeSessionId);
                if (session.Status != "complete" || session.PaymentStatus != "paid")
                {
                    // Pagamento non completato (l'utente potrebbe aver annullato)
                    order.Status = OrderStatus.Cancelled;
                    await orderRepository.UpdateAsync(order);

                    logger.LogWarning("Pagamento non completato per ordine {OrderId}", order.Id);
                    throw new ApiException(HttpStatusCode.InternalServerError, "Pagamento non completato o annullato.");
                }

                // 3. Pagamento riuscito - aggiorna stato ordine
                order.Status = OrderStatus.Paid;
                await orderRepository.UpdateAsync(order);

                await emailSender.SendAsync(new(null, null, null, GetHtmlMessage($"{env.WebRootPath}/mailTemplates/order_paid.html"))
                {
                    To = [new EmailAddress(order.CustomerEmail, $"{order.CustomerLastName}, {order.CustomerFirstName}")],
                    Subject = "Conferma di pagamento - Ordine #" + order.Id
                });

                // 4. Aggiorna lo stato di ogni quadro come venduto (IsForSale = false)
                foreach (var item in order.Items)
                {
                    var painting = await paintingRepository.GetByIdAsync(item.PaintingId);
                    if (painting != null)
                    {
                        painting.IsForSale = false;
                        await paintingRepository.UpdateAsync(painting);
                    }
                }
                logger.LogInformation("Ordine {OrderId} confermato come PAGATO.", order.Id);
                return Result<OrderDto>.Success(order.ToOrderAdminDto());
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Errore nel recupero della sessione Stripe {SessionId}", stripeSessionId);
                throw new ApiException(HttpStatusCode.InternalServerError, "Impossibile verificare il pagamento con Stripe. Riprova più tardi.");
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await orderRepository.GetAllAsync();
            return orders.Select(o => o.ToOrderAdminDto());
        }

        public async Task<OrderDto> UpdateOrderTrackingAsync(int orderId, string trackingNumber)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new ApiException(HttpStatusCode.NotFound, $"Ordine ID {orderId} non trovato.");

            order.TrackingNumber = trackingNumber;
            await orderRepository.UpdateAsync(order);
            return order.ToOrderAdminDto();
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new ApiException(HttpStatusCode.NotFound, $"Ordine ID {orderId} non trovato.");

            var prevStatus = order.Status;
            order.Status = newStatus;
            await orderRepository.UpdateAsync(order);

            if (newStatus == OrderStatus.Paid && prevStatus != OrderStatus.Paid)
            {
                foreach (var item in order.Items)
                {
                    var painting = await paintingRepository.GetByIdAsync(item.PaintingId);
                    if (painting != null)
                    {
                        painting.IsForSale = false;
                        await paintingRepository.UpdateAsync(painting);
                    }
                }
            }
            else if (newStatus == OrderStatus.Cancelled && prevStatus == OrderStatus.Paid)
            {
                foreach (var item in order.Items)
                {
                    var painting = await paintingRepository.GetByIdAsync(item.PaintingId);
                    if (painting != null)
                    {
                        painting.IsForSale = true;
                        await paintingRepository.UpdateAsync(painting);
                    }
                }
            }

            return order.ToOrderAdminDto();
        }

        private static string GetHtmlMessage(string templatePath)
        {
            return System.IO.File.ReadAllText(templatePath);
        }
    }
}
