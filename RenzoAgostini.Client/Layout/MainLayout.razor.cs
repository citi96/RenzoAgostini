using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private IKeycloakService KeycloakService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ICartService CartService { get; set; } = default!;

        private int cartCount = 0;
        private bool showMobileMenu = false;

        protected override void OnInitialized()
        {
            cartCount = CartService.ItemsCount;
            CartService.OnChange += HandleCartChanged;
        }

        private void HandleCartChanged()
        {
            var newCount = CartService.ItemsCount;

            // Trigger animation if count increased
            if (newCount > cartCount)
            {
                InvokeAsync(async () =>
                {
                    StateHasChanged();
                    await Task.Delay(100); // Small delay to ensure DOM is updated
                    await JSRuntime.InvokeVoidAsync("addBounceAnimation", ".nav-cart-count");
                });
            }

            cartCount = newCount;
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            CartService.OnChange -= HandleCartChanged;
        }

        private async void ShowLoginModal()
        {
            try
            {
                var loginUrl = await KeycloakService.GetLoginUrlAsync();
                await JSRuntime.InvokeVoidAsync("open", loginUrl, "_self");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore Keycloak login: {ex}");
                // Qui potresti mostrare un toast di errore
                await ShowToast("Errore durante il login", "error");
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                if (AuthProvider is CustomAuthenticationStateProvider customProvider)
                {
                    await customProvider.LogoutAsync();
                }

                var logoutUrl = KeycloakService.GetLogoutUrl();
                await JSRuntime.InvokeVoidAsync("open", logoutUrl, "_self");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il logout: {ex}");
                await ShowToast("Errore durante il logout", "error");
            }
        }

        private void ToggleMobileMenu()
        {
            showMobileMenu = !showMobileMenu;
            StateHasChanged();
        }

        private void CloseMobileMenu()
        {
            showMobileMenu = false;
            StateHasChanged();
        }

        // Metodo helper per mostrare toast notifications
        private async Task ShowToast(string message, string type = "info", string title = "")
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, type, title);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel mostrare il toast: {ex}");
            }
        }

        // Metodi per gestire eventi della tastiera (accessibilità)
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //await JSRuntime.InvokeVoidAsync("initializeLayout");
            }
        }
    }
}