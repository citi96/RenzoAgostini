using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Pages
{
    public partial class CheckoutCancel
    {
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] ICheckoutService CheckoutClient { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;

        private bool isProcessing = true;
        private string? error;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var sessionId = GetQuery("session_id");
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    // Conferma il pagamento anche se cancellato per aggiornare lo stato
                    await CheckoutClient.ConfirmPaymentAsync(sessionId);
                }

                // Non svuotiamo il carrello in caso di cancellazione
                // così l'utente può riprovare facilmente
            }
            catch (Exception ex)
            {
                error = "Si è verificato un errore durante l'elaborazione della cancellazione: " + ex.Message;
            }
            finally
            {
                isProcessing = false;
            }
        }

        private string GetQuery(string key)
        {
            var uri = new Uri(Navigation.Uri);
            var q = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return q[key] ?? string.Empty;
        }

        private void GoToHome() =>
            Navigation.NavigateTo("/");

        private void GoToCart() =>
            Navigation.NavigateTo("/cart");
    }
}