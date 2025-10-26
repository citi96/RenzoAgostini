using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Authentication
{
    public class CustomAuthenticationStateProvider(ICookieService cookieService, IConfiguration configuration) : AuthenticationStateProvider, IDisposable
    {
        public UserDto? CurrentUser { get; set; }

        public void Dispose()
        {
            AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;
            GC.SuppressFinalize(this);
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await cookieService.GetAsync<string>("access_token");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var principal = CreatePrincipalFromKeycloakToken(token);
            CurrentUser = UserDto.FromClaimsPrincipal(principal);

            return new AuthenticationState(principal);
        }

        private ClaimsPrincipal CreatePrincipalFromKeycloakToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var claims = new List<Claim>();

            claims.AddRange(token.Claims);
                        
            if (token.Payload.TryGetValue("resource_access", out var res))
            {
                using var doc = JsonDocument.Parse(res.ToString()!);
                if (doc.RootElement.TryGetProperty(configuration["Keycloak:ClientId"] ?? string.Empty, out var client) &&
                    client.TryGetProperty("roles", out var arr2))
                    foreach (var r in arr2.EnumerateArray())
                        claims.Add(new Claim(ClaimTypes.Role, r.GetString()!));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }


        public async Task LogoutAsync()
        {
            CurrentUser = null;
            await cookieService.ClearAsync("access_token");
            await cookieService.ClearAsync("auth_state");
            await cookieService.ClearAsync("auth_nonce");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }

        public void UpdateCurrentUser(string token)
        {
            var principal = CreatePrincipalFromKeycloakToken(token);
            CurrentUser = UserDto.FromClaimsPrincipal(principal);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
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