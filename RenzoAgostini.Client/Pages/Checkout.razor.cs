using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class Checkout : ComponentBase
    {
        [Inject] ICheckoutClient CheckoutClient { get; set; } = default!;
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;

        private string? errorMessage;

        protected override void OnInitialized()
        {
            if (CartService.CheckoutData == null)
            {
                errorMessage = "Nessun ordine da processare.";
            }
        }

        private async Task StartPayment()
        {
            if (CartService.CheckoutData == null)
                return;

            try
            {
                var session = await CheckoutClient.CreateCheckoutSessionAsync(CartService.CheckoutData!);
                Navigation.NavigateTo(session.SessionUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                errorMessage = "Si è verificato un errore imprevisto. Riprova.";
                Console.Error.WriteLine("StartPayment error: " + ex.Message);
            }
        }

        private decimal GetShippingCost()
        {
            decimal shippingCost = 0m;
            string method = CartService.CheckoutData.ShippingMethod ?? "Standard";
            bool domestic = CartService.CheckoutData.Country != null &&
                            CartService.CheckoutData.Country.ToLower().Contains("ital");
            // Esempio: costo 10€ domestico, 25€ estero per Standard; Express: 20€ domestico, 50€ estero
            if (method.Equals("Express", StringComparison.OrdinalIgnoreCase))
                shippingCost = domestic ? 20m : 50m;
            else
                shippingCost = domestic ? 10m : 25m;

            return shippingCost;
        }

        private void NavigateToGallery()
        {
            Navigation.NavigateTo("/");
        }
    }
}
