using System.Net;
using Microsoft.Extensions.Options;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Emailing.Interfaces;
using RenzoAgostini.Server.Emailing.Models;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Exceptions;
using RenzoAgostini.Server.Mappings;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services
{
    public class CustomOrderService(
        ICustomOrderRepository customOrderRepository,
        IPaintingRepository paintingRepository,
        ICustomEmailSender emailSender,
        IWebHostEnvironment env,
        IOptions<StorageOptions> storageOptions,
        IOptions<EmailOptions> emailOptions,
        ILogger<CustomOrderService> logger) : ICustomOrderService
    {
        private readonly string _customOrdersPath = ResolveCustomOrdersPath(env, storageOptions.Value);
        private readonly EmailOptions _emailOptions = emailOptions.Value;

        public async Task<CustomOrderDto> CreateCustomOrderAsync(CreateCustomOrderDto dto)
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
                    AttachmentPath = dto.AttachmentPath,
                    AttachmentOriginalName = dto.AttachmentOriginalName
                };

                await customOrderRepository.AddAsync(customOrder);

                await SendCustomerConfirmationEmail(customOrder);
                await SendArtistNotificationEmail(customOrder);

                logger.LogInformation("Creata richiesta personalizzata {CustomOrderId}", customOrder.Id);
                return customOrder.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nella creazione della richiesta personalizzata");
                throw;
            }
        }

        public async Task<CustomOrderDto> AcceptCustomOrderAsync(int customOrderId, AcceptCustomOrderDto dto)
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
                return customOrder.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nell'accettazione della richiesta {CustomOrderId}", customOrderId);
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nell'accettazione della richiesta {customOrderId}");
            }
        }

        public async Task<CustomOrderDto> RejectCustomOrderAsync(int customOrderId, string? reason)
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
                return customOrder.ToDto();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel rifiuto della richiesta {CustomOrderId}", customOrderId);
                throw new ApiException(HttpStatusCode.InternalServerError, $"Errore nel rifiuto della richiesta {customOrderId}");
            }
        }

        public async Task<CustomOrderDto> GetByAccessCodeAsync(string accessCode, string customerEmail)
        {
            try
            {
                var customOrder = await customOrderRepository.GetByAccessCodeAndEmailAsync(accessCode, customerEmail);

                if (customOrder == null)
                    throw new ApiException(HttpStatusCode.NotFound, "Codice di accesso non valido o email non corrispondente");

                //if (customOrder.Status != CustomOrderStatus.Accepted)
                //    throw new ApiException(HttpStatusCode.BadRequest, "La richiesta non è ancora stata accettata");

                return customOrder.ToDto();
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
            Directory.CreateDirectory(_customOrdersPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_customOrdersPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/custom-orders/{fileName}";
        }

        private async Task SendCustomerConfirmationEmail(CustomOrder customOrder)
        {
            var message = new EmailMessage(new EmailAddress(_emailOptions.NoReplySender, "Renzo Agostini"), null, null, $@"
                <h2>Richiesta Ricevuta</h2>
                <p>Salve,</p>
                <p>Ho ricevuto la tua richiesta per un quadro personalizzato.</p>
                <p><strong>Descrizione:</strong> {customOrder.Description}</p>
                <p>Ti contatterò presto con maggiori dettagli.</p>
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
            var artistEmail = _emailOptions.ArtistNotificationRecipient;
            var senderEmail = _emailOptions.NoReplySender;

            var attachmentInfo = customOrder.AttachmentPath != null
                ? $"<p><strong>Allegato:</strong> {customOrder.AttachmentOriginalName}</p>"
                : "<p>Nessun allegato</p>";

            var message = new EmailMessage(new EmailAddress(senderEmail, "Renzo Agostini System"), null, null, $@"
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
            var message = new EmailMessage(new EmailAddress(_emailOptions.NoReplySender, "Renzo Agostini"), null, null, $@"
                <h2>Richiesta Accettata!</h2>
                <p>Salve,</p>
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

            var message = new EmailMessage(new EmailAddress(_emailOptions.NoReplySender, "Renzo Agostini"), null, null, $@"
                <h2>Richiesta Non Accettata</h2>
                <p>Salve,</p>
                <p>Purtroppo non posso procedere con la tua richiesta di quadro personalizzato.</p>
                {reasonText}
                <p>Ti ringrazio per l'interesse.</p>
            ")
            {
                To = [new EmailAddress(customOrder.CustomerEmail)],
                Subject = "Quadro Personalizzato - Richiesta"
            };

            await emailSender.SendAsync(message);
        }

        private static string ResolveCustomOrdersPath(IWebHostEnvironment environment, StorageOptions options)
        {
            var configuredPath = options.CustomOrdersPath;
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.Combine(environment.WebRootPath, "custom-orders");
            }

            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);
        }
    }
}