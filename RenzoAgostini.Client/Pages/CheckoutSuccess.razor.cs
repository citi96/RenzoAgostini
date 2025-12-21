using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Pages
{
    public partial class CheckoutSuccess
    {
        [Inject] ICheckoutService CheckoutClient { get; set; } = default!;
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;

        private bool isProcessing = true;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            // Estrae session_id dalla querystring
            string sessionId = GetSessionIdFromQuery();
            if (string.IsNullOrEmpty(sessionId))
            {
                errorMessage = "ID sessione mancante.";
                isProcessing = false;
                return;
            }

            try
            {
                await CheckoutClient.ConfirmPaymentAsync(sessionId);
                await CartService.ClearAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Errore conferma pagamento: " + ex.Message);
                errorMessage = "Errore di comunicazione nella conferma dell'ordine.";
            }
            finally
            {
                isProcessing = false;
                try 
                { 
                    await LocalClearCart(); 
                } catch 
                { 
                    /* ignore */ 
                }
            }
        }

        private string GetSessionIdFromQuery()
        {
            var uri = new Uri(Navigation.Uri);
            var query = uri.Query; // e.g. "?session_id=cs_test_12345"
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;
            var param = query.TrimStart('?').Split('&').FirstOrDefault(p => p.StartsWith("session_id="));
            return param?["session_id=".Length..] ?? string.Empty;
        }

        private void GoToHome() => 
            Navigation.NavigateTo("/");

        // Facoltativo: metodo per svuotare il carrello lato client
        private Task LocalClearCart()
        {
            // Poiché non abbiamo CartService qui iniettato, potremmo navigare in un'area dove CartService viene ripulito.
            // Ad esempio, potremmo navigare alla home e nello StateHasChanged del MainLayout svuotare il carrello.
            return Task.CompletedTask;
        }
    }
}
