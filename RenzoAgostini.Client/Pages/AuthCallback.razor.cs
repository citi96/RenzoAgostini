using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Pages
{
    public partial class AuthCallback : ComponentBase
    {
        [Inject] private IKeycloakService KeycloakService { get; set; } = default!;
        [Inject] private CustomAuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!; 


        private bool isProcessing = true;
        private bool hasError = false;
        private string errorMessage = "";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var uri = new Uri(Navigation.Uri);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                var code = query["code"];
                var state = query["state"];
                var error = query["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    HandleAuthError(error, query["error_description"]);
                    return;
                }

                if (string.IsNullOrEmpty(code))
                {
                    HandleAuthError("missing_code", "Codice di autorizzazione mancante");
                    return;
                }

                var token = await KeycloakService.HandleCallbackAsync(code);
                if (token != null)
                {
                    // Aggiorna il provider di autenticazione
                    if (AuthProvider is CustomAuthenticationStateProvider customProvider)
                        customProvider.UpdateCurrentUser(token);

                    ReturnHome();
                }
                else
                {
                    HandleAuthError("auth_failed", "Autenticazione fallita");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il callback: {ex.Message}");
                HandleAuthError("callback_error", "Errore durante il processo di autenticazione");
            }
        }

        private void HandleAuthError(string error, string? description = null)
        {
            isProcessing = false;
            hasError = true;
            errorMessage = description ?? GetErrorDescription(error);
            StateHasChanged();
        }

        private string GetErrorDescription(string error)
        {
            return error switch
            {
                "access_denied" => "Accesso negato dall'utente.",
                "invalid_request" => "Richiesta non valida.",
                "unauthorized_client" => "Client non autorizzato.",
                "unsupported_response_type" => "Tipo di risposta non supportato.",
                "invalid_scope" => "Scope non valido.",
                "server_error" => "Errore del server di autenticazione.",
                "temporarily_unavailable" => "Servizio temporaneamente non disponibile.",
                _ => "Si è verificato un errore durante l'autenticazione."
            };
        }

        private void ReturnHome()
        {
            Navigation.NavigateTo("/", true);
        }

    }
}
