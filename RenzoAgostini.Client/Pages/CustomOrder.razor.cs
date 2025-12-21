// RenzoAgostini.Client/Pages/CustomOrder.razor.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;
using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Client.Pages
{
    public partial class CustomOrder : ComponentBase
    {
        [Inject] private IImageUploadService ImageUploadService { get; set; } = default!;
        [Inject] private ICustomOrderService CustomOrderService { get; set; } = default!;
        [Inject] private ICookieService CookieService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] CustomAuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private ILogger<CustomOrder> Logger { get; set; } = default!;

        protected CustomOrderRequestModel customOrderRequest = new();
        protected AccessCustomOrderModel accessRequest = new();
        protected IBrowserFile? selectedFile;
        protected string? selectedFileName;
        protected string? successMessage;
        protected string? errorMessage;
        protected string? accessErrorMessage;
        protected string? accessStatusMessage;
        protected string? accessCode;
        protected bool isSubmitting = false;
        protected bool isAccessingOrder = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var state = await AuthProvider.GetAuthenticationStateAsync();
                if (state.User.Identity?.IsAuthenticated == true)
                {
                    var user = state.User;
                    customOrderRequest.CustomerEmail = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
                    accessRequest.CustomerEmail = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error setting logged user email");
            }
        }

        protected void HandleFileChange(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            selectedFileName = e.File?.Name;
            StateHasChanged();
        }

        protected async Task SubmitRequest()
        {
            const long maxFileSize = 5 * 1024 * 1024;

            try
            {
                isSubmitting = true;
                errorMessage = null;
                StateHasChanged();

                var filePath = string.Empty;
                if (selectedFile != null)
                {
                    using var stream = selectedFile.OpenReadStream(maxFileSize);
                    filePath = await ImageUploadService.UploadImageAsync(stream, selectedFile.Name);
                }

                var dto = new CreateCustomOrderDto(
                    customOrderRequest.CustomerEmail,
                    customOrderRequest.Description,
                    filePath,
                    selectedFileName
                );

                var result = await CustomOrderService.CreateCustomOrderAsync(dto);

                accessCode = result.AccessCode;
                successMessage = "La tua richiesta è stata inviata con successo! Ti contatteremo presto via email.";
                await JSRuntime.InvokeVoidAsync("window.scrollTo", 0, 0);

                // Reset form
                ResetForm();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error submitting custom order request");
                errorMessage = ex.Message;
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }

        protected async Task AccessCustomOrder()
        {
            try
            {
                isAccessingOrder = true;
                accessErrorMessage = null;
                accessStatusMessage = null;
                StateHasChanged();

                var result = await CustomOrderService.GetByAccessCodeAsync(
                    accessRequest.AccessCode,
                    accessRequest.CustomerEmail
                );

                if (result.PaintingId.HasValue)
                {
                    // Reindirizza alla pagina del quadro
                    await CookieService.PutAsync("customOrder", JsonSerializer.Serialize(result));
                    await JSRuntime.InvokeVoidAsync("open", $"/quadro-personalizzato", "_self");
                }
                else
                {
                    if (result.Status == CustomOrderStatus.Pending)
                    {
                        accessStatusMessage = "La tua richiesta è stata ricevuta ed è in fase di valutazione. Ti notificheremo via email quando sarà accettata.";
                    }
                    else if (result.Status == CustomOrderStatus.Rejected)
                    {
                        accessErrorMessage = "Ci dispiace, ma la tua richiesta è stata rifiutata dall'artista. Controlla la tua email per maggiori dettagli.";
                    }
                    else
                    {
                        accessStatusMessage = "La tua richiesta è stata accettata! Stiamo preparando il tuo quadro personalizzato. Riprova tra poco.";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accessing custom order");
                accessErrorMessage = ex.Message;
            }
            finally
            {
                isAccessingOrder = false;
                StateHasChanged();
            }
        }

        private void ResetForm()
        {
            customOrderRequest = new();
            selectedFile = null;
            selectedFileName = null;
            StateHasChanged();
        }

        public class CustomOrderRequestModel
        {
            [Required(ErrorMessage = "Email obbligatoria")]
            [EmailAddress(ErrorMessage = "Formato email non valido")]
            public string CustomerEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Descrizione obbligatoria")]
            [MinLength(10, ErrorMessage = "La descrizione deve essere di almeno 10 caratteri")]
            [MaxLength(2000, ErrorMessage = "La descrizione non può superare i 2000 caratteri")]
            public string Description { get; set; } = string.Empty;

            [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare l'informativa sulla privacy.")]
            public bool PrivacyAccepted { get; set; }
        }

        public class AccessCustomOrderModel
        {
            [Required(ErrorMessage = "Codice obbligatorio")]
            public string AccessCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email obbligatoria")]
            [EmailAddress(ErrorMessage = "Formato email non valido")]
            public string CustomerEmail { get; set; } = string.Empty;
        }
    }
}