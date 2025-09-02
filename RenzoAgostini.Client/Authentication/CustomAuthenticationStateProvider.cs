using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Authentication.Responses;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Authentication
{
    public class CustomAuthenticationStateProvider(IUserService userService, ICookieService cookieService) : AuthenticationStateProvider, IDisposable
    {
        public UserDto? CurrentUser { get; private set; }

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
                var authenticatedUser = await userService.SendAuthenticateRequestAsync(user.UserName, user.PasswordHash ?? string.Empty);

                if (authenticatedUser != null)
                {
                    principal = authenticatedUser.ToClaimsPrincipal();
                    CurrentUser = authenticatedUser;
                }
            }

            return new(principal);
        }

        public async Task LoginAsync(string username, string password)
        {
            var principal = new ClaimsPrincipal();
            var user = await userService.SendAuthenticateRequestAsync(username, password);

            if (user != null)
            {
                CurrentUser = user;
                principal = user.ToClaimsPrincipal();
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public async Task LogoutAsync()
        {
            await cookieService.ClearAsync("token");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        }

        [JSInvokable]
        public async void GoogleLogin(GoogleResponse googleResponse)
        {
            var principal = new ClaimsPrincipal();
            var user = await userService.SendAuthenticateRequestAsync(EProvider.Google, googleResponse.Credential);

            if (user != null)
            {
                CurrentUser = user;
                principal = user.ToClaimsPrincipal();
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
        {
            var authenticationState = await task;

            if (authenticationState != null)
                CurrentUser = UserDto.FromClaimsPrincipal(authenticationState.User);
        }
    }
}
