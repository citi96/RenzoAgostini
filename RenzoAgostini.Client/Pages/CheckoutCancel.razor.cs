using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Pages
{
    public partial class CheckoutCancel
    {
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] ICheckoutClient CheckoutClient { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;

        private bool isProcessing = true;
        private string? error;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var sessionId = GetQuery("session_id");
                if (!string.IsNullOrWhiteSpace(sessionId))
                    await CheckoutClient.ConfirmPaymentAsync(sessionId);

                CartService.Clear(); // svuota carrello locale
            }
            catch (Exception ex)
            {
                error = ex.Message;
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
    }
}
