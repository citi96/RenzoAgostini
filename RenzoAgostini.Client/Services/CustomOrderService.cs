using System.Net.Http.Json;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class CustomOrderService(HttpClient httpClient, ILogger<CustomOrderService> logger) : ICustomOrderService
    {
        public async Task<CustomOrderDto> CreateCustomOrderAsync(CreateCustomOrderDto dto)
        {
            try
            {
                using var content = new MultipartFormDataContent
            {
                { new StringContent(dto.CustomerEmail), nameof(CreateCustomOrderDto.CustomerEmail) },
                { new StringContent(dto.Description), nameof(CreateCustomOrderDto.Description) }
            };

                var response = await httpClient.PostAsync("/api/customorders", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CustomOrderDto>();
                return result ?? throw new InvalidOperationException("Empty response from server");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error creating custom order");
                throw new ApplicationException("Errore di comunicazione con il server", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating custom order");
                throw new ApplicationException("Errore nella creazione della richiesta", ex);
            }
        }

        public async Task<CustomOrderDto> GetByAccessCodeAsync(string accessCode, string customerEmail)
        {
            try
            {
                var dto = new AccessCustomOrderDto(accessCode.Trim().ToUpper(), customerEmail.Trim());
                var response = await httpClient.PostAsJsonAsync("/api/customorders/access", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new ApplicationException($"Codice non valido o email non corrispondente: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<CustomOrderDto>();
                return result ?? throw new InvalidOperationException("Empty response from server");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error accessing custom order with code {AccessCode}", accessCode);
                throw new ApplicationException("Errore di comunicazione con il server", ex);
            }
            catch (ApplicationException)
            {
                throw; // Re-throw application exceptions as-is
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing custom order with code {AccessCode}", accessCode);
                throw new ApplicationException("Errore nella verifica del codice", ex);
            }
        }

        public async Task<IEnumerable<CustomOrderDto>> GetAllCustomOrdersAsync()
        {
            try
            {
                var result = await httpClient.GetFromJsonAsync<IEnumerable<CustomOrderDto>>("/api/customorders");
                return result ?? Array.Empty<CustomOrderDto>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error getting all custom orders");
                throw new ApplicationException("Errore di comunicazione con il server", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting all custom orders");
                throw new ApplicationException("Errore nel caricamento delle richieste", ex);
            }
        }

        public async Task<CustomOrderDto> AcceptCustomOrderAsync(int customOrderId, AcceptCustomOrderDto dto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"/api/customorders/{customOrderId}/accept", dto);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CustomOrderDto>();
                return result ?? throw new InvalidOperationException("Empty response from server");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error accepting custom order {OrderId}", customOrderId);
                throw new ApplicationException("Errore di comunicazione con il server", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accepting custom order {OrderId}", customOrderId);
                throw new ApplicationException("Errore nell'accettazione della richiesta", ex);
            }
        }

        public async Task<CustomOrderDto> RejectCustomOrderAsync(int customOrderId, string? reason)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"/api/customorders/{customOrderId}/reject", reason);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CustomOrderDto>();
                return result ?? throw new InvalidOperationException("Empty response from server");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error rejecting custom order {OrderId}", customOrderId);
                throw new ApplicationException("Errore di comunicazione con il server", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rejecting custom order {OrderId}", customOrderId);
                throw new ApplicationException("Errore nel rifiuto della richiesta", ex);
            }
        }
    }
}
