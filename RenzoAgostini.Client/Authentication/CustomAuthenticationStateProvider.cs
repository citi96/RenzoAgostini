using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Authentication
{
    public class CustomAuthenticationStateProvider(IUserService userService, ICookieService cookieService) : AuthenticationStateProvider, IDisposable
    {
        public UserDto? CurrentUser { get; set; }

        public void Dispose()
        {
            AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
            GC.SuppressFinalize(this);
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();

            var user = await userService.FetchUserFromBrowserAsync();
            if (user != null)
            {
                var authenticatedUser = await userService.SendAuthenticateRequestAsync(await cookieService.GetAsync<string>("access_token"));

                if (authenticatedUser != null)
                {
                    principal = authenticatedUser.ToClaimsPrincipal();
                    CurrentUser = authenticatedUser;
                }
            }

            return new AuthenticationState(principal);
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            await cookieService.ClearAsync("access_token");
            await cookieService.ClearAsync("auth_state");
            await cookieService.ClearAsync("auth_nonce");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }

        public void UpdateCurrentUser(UserDto user)
        {
            CurrentUser = user;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user.ToClaimsPrincipal())));
        }

        private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
        {
            try
            {
                var authenticationState = await task;
                if (authenticationState?.User?.Identity?.IsAuthenticated == true)
                {
                    CurrentUser = UserDto.FromClaimsPrincipal(authenticationState.User);
                }
                else
                {
                    CurrentUser = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel cambio stato di autenticazione: {ex.Message}");
                CurrentUser = null;
            }
        }
    }
}