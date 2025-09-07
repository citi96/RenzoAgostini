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
        [Inject] ICartService CartService { get; set; } = default!;

        private int cartCount = 0;

        protected override void OnInitialized()
        {
            cartCount = CartService.ItemsCount;
            CartService.OnChange += HandleCartChanged;
        }

        private void HandleCartChanged()
        {
            cartCount = CartService.ItemsCount;
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
                StateHasChanged();

                var loginUrl = await KeycloakService.GetLoginUrlAsync();
                await JSRuntime.InvokeVoidAsync("open", loginUrl, "_self");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore Keycloak login: {ex}");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task LogoutAsync()
        {
            if (AuthProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.LogoutAsync();
            }

            var logoutUrl = KeycloakService.GetLogoutUrl();
            await JSRuntime.InvokeVoidAsync("open", logoutUrl, "_self");
        }
    }
}
