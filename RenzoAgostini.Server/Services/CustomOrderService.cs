﻿using System.Net;
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

namespace RenzoAgostini.Server.Services
{
    public class CustomOrderService(
        ICustomOrderRepository customOrderRepository,
        IPaintingRepository paintingRepository,
        ICustomEmailSender emailSender,
        IWebHostEnvironment env,
        ILogger<CustomOrderService> logger) : ICustomOrderService
    {
        private readonly string _uploadsPath = Path.Combine(env.WebRootPath, "custom-orders");

        public async Task<Result<CustomOrderDto>> CreateCustomOrderAsync(CreateCustomOrderDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
                    throw new ApiException(HttpStatusCode.BadRequest, "Email obbligatoria");
                if (string.IsNullOrWhiteSpace(dto.Description))
                    throw new ApiException(HttpStatusCode.BadRequest, "Descrizione obbligatoria");

                var customOrder = new CustomOrder
                {
                    CustomerEmail = dto.CustomerEmail.Trim(),
                    Description = dto.Description.Trim(),
                    AccessCode = GenerateAccessCode(),
                    Status = CustomOrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    AttachmentPath = dto.AttachmentPath
                };

                await customOrderRepository.AddAsync(customOrder);

                await SendCustomerConfirmationEmail(customOrder);
                await SendArtistNotificationEmail(customOrder);

                logger.LogInformation("Creata richiesta personalizzata {CustomOrderId}", customOrder.Id);
                return Result<CustomOrderDto>.Success(customOrder.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nella creazione della richiesta personalizzata");
                throw;
            }
        }

        public async Task<Result<CustomOrderDto>> AcceptCustomOrderAsync(int customOrderId, AcceptCustomOrderDto dto)
        {
            try
            {
                var customOrder = await customOrderRepository.GetByIdAsync(customOrderId)
                    ?? throw new ApiException(HttpStatusCode.NotFound, "Richiesta non trovata");

                if (customOrder.Status != CustomOrderStatus.Pending)
                    throw new ApiException(HttpStatusCode.BadRequest, "La richiesta non è in stato pending");

                var painting = dto.PaintingData.ToEntity();
                painting.IsForSale = true; // Per questo cliente specifico è sempre "in vendita"
                painting.Price = dto.QuotedPrice;

                var createdPainting = await paintingRepository.AddAsync(painting);

                customOrder.Status = CustomOrderStatus.Accepted;
                customOrder.AcceptedAt = DateTime.UtcNow;
                customOrder.QuotedPrice = dto.QuotedPrice;
                customOrder.ArtistNotes = dto.ArtistNotes;
                customOrder.PaintingId = createdPainting.Id;

                await customOrderRepository.UpdateAsync(customOrder);

                await SendAcceptanceEmailToCustomer(customOrder);

                logger.LogInformation("Richiesta personalizzata {CustomOrderId} accettata", customOrderId);
                return Result<CustomOrderDto>.Success(customOrder.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nell'accettazione della richiesta {CustomOrderId}", customOrderId);
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nell'accettazione della richiesta {customOrderId}");
            }
        }

        public async Task<Result<CustomOrderDto>> RejectCustomOrderAsync(int customOrderId, string? reason)
        {
            try
            {
                var customOrder = await customOrderRepository.GetByIdAsync(customOrderId)
                    ?? throw new ApiException(HttpStatusCode.NotFound, "Richiesta non trovata");

                if (customOrder.Status != CustomOrderStatus.Pending)
                    throw new ApiException(HttpStatusCode.BadRequest, "La richiesta non è in stato pending");

                customOrder.Status = CustomOrderStatus.Rejected;
                customOrder.ArtistNotes = reason;
                await customOrderRepository.UpdateAsync(customOrder);

                await SendRejectionEmailToCustomer(customOrder);

                logger.LogInformation("Richiesta personalizzata {CustomOrderId} rifiutata", customOrderId);
                return Result<CustomOrderDto>.Success(customOrder.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel rifiuto della richiesta {CustomOrderId}", customOrderId);
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nel rifiuto della richiesta {customOrderId}");
            }
        }

        public async Task<Result<CustomOrderDto>> GetByAccessCodeAsync(string accessCode, string customerEmail)
        {
            try
            {
                var customOrder = await customOrderRepository.GetByAccessCodeAndEmailAsync(accessCode, customerEmail);

                if (customOrder == null)
                    return Result<CustomOrderDto>.Failure("Codice di accesso non valido o email non corrispondente");

                if (customOrder.Status != CustomOrderStatus.Accepted)
                    return Result<CustomOrderDto>.Failure("La richiesta non è ancora stata accettata");

                return Result<CustomOrderDto>.Success(customOrder.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero della richiesta con codice {AccessCode}", accessCode);
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nel recupero della richiesta con codice {accessCode}");
            }
        }

        public async Task<IEnumerable<CustomOrderDto>> GetAllCustomOrdersAsync()
        {
            try
            {
                var customOrders = await customOrderRepository.GetAllAsync();
                return customOrders.Select(co => co.ToDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero delle richieste personalizzate");
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nel recupero delle richieste personalizzate");
            }
        }

        private static string GenerateAccessCode()
        {
            return $"CO{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }

        private async Task<string> SaveAttachmentAsync(IFormFile file)
        {
            Directory.CreateDirectory(_uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/custom-orders/{fileName}";
        }

        private async Task SendCustomerConfirmationEmail(CustomOrder customOrder)
        {
            var message = new EmailMessage(null, null, null, $@"
                <h2>Richiesta Ricevuta</h2>
                <p>Ciao,</p>
                <p>Abbiamo ricevuto la tua richiesta per un quadro personalizzato.</p>
                <p><strong>Descrizione:</strong> {customOrder.Description}</p>
                <p>Ti contatteremo presto con maggiori dettagli.</p>
                <p>Il tuo codice di riferimento è: <strong>{customOrder.AccessCode}</strong></p>
            ")
            {
                To = [new EmailAddress(customOrder.CustomerEmail)],
                Subject = "Richiesta Quadro Personalizzato - Ricevuta"
            };

            await emailSender.SendAsync(message);
        }

        private async Task SendArtistNotificationEmail(CustomOrder customOrder)
        {
            // Email per l'artista - configurabile
            var artistEmail = "renzo.agostini@example.com"; // TODO: da configuration

            var attachmentInfo = customOrder.AttachmentPath != null
                ? $"<p><strong>Allegato:</strong> {customOrder.AttachmentOriginalName}</p>"
                : "<p>Nessun allegato</p>";

            var message = new EmailMessage(null, null, null, $@"
                <h2>Nuova Richiesta Quadro Personalizzato</h2>
                <p><strong>Cliente:</strong> {customOrder.CustomerEmail}</p>
                <p><strong>Descrizione:</strong> {customOrder.Description}</p>
                {attachmentInfo}
                <p><strong>Codice:</strong> {customOrder.AccessCode}</p>
                <p>Accedi al pannello admin per gestire la richiesta.</p>
            ")
            {
                To = [new EmailAddress(artistEmail)],
                Subject = $"Nuova Richiesta Personalizzata - {customOrder.AccessCode}"
            };

            await emailSender.SendAsync(message);
        }

        private async Task SendAcceptanceEmailToCustomer(CustomOrder customOrder)
        {
            var message = new EmailMessage(null, null, null, $@"
                <h2>Richiesta Accettata!</h2>
                <p>Ciao,</p>
                <p>La tua richiesta è stata accettata dall'artista!</p>
                <p><strong>Prezzo:</strong> €{customOrder.QuotedPrice:F2}</p>
                {(string.IsNullOrEmpty(customOrder.ArtistNotes) ? "" : $"<p><strong>Note dell'artista:</strong> {customOrder.ArtistNotes}</p>")}
                <p>Per procedere all'acquisto, usa questo codice nel sito:</p>
                <p><strong>Codice di accesso:</strong> {customOrder.AccessCode}</p>
                <p>Inserisci il codice insieme alla tua email nella sezione ""Hai già ordinato un quadro personalizzato?""</p>
            ")
            {
                To = [new EmailAddress(customOrder.CustomerEmail)],
                Subject = "Quadro Personalizzato - Richiesta Accettata!"
            };

            await emailSender.SendAsync(message);
        }

        private async Task SendRejectionEmailToCustomer(CustomOrder customOrder)
        {
            var reasonText = string.IsNullOrEmpty(customOrder.ArtistNotes)
                ? ""
                : $"<p><strong>Motivo:</strong> {customOrder.ArtistNotes}</p>";

            var message = new EmailMessage(null, null, null, $@"
                <h2>Richiesta Non Accettata</h2>
                <p>Ciao,</p>
                <p>Purtroppo non possiamo procedere con la tua richiesta di quadro personalizzato.</p>
                {reasonText}
                <p>Ti ringraziamo per l'interesse e speriamo di poterti servire in futuro.</p>
            ")
            {
                To = [new EmailAddress(customOrder.CustomerEmail)],
                Subject = "Quadro Personalizzato - Richiesta"
            };

            await emailSender.SendAsync(message);
        }
    }
}
