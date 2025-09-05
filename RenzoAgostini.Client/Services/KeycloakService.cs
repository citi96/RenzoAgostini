using System.Text.Json;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly ICookieService _cookieService;
        private readonly IConfiguration _configuration;

        private readonly string _keycloakUrl;
        private readonly string _realm;
        private readonly string _clientId;
        private readonly string _redirectUri;

        public KeycloakService(HttpClient httpClient, ICookieService cookieService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cookieService = cookieService;
            _configuration = configuration;

            // Configura questi valori nel tuo appsettings.json o come variabili d'ambiente
            _keycloakUrl = _configuration["Keycloak:Url"] ?? "https://your-keycloak-server.com";
            _realm = _configuration["Keycloak:Realm"] ?? "your-realm";
            _clientId = _configuration["Keycloak:ClientId"] ?? "your-client-id";
            _redirectUri = _configuration["Keycloak:RedirectUri"] ?? "https://localhost:7189/auth/callback";
        }

        public async Task<string> GetLoginUrlAsync()
        {
            var state = Guid.NewGuid().ToString();
            var nonce = Guid.NewGuid().ToString();

            // Salva state per verificarlo dopo il callback
            await _cookieService.PutAsync("auth_state", state);
            await _cookieService.PutAsync("auth_nonce", nonce);

            var loginUrl = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/auth" +
                          $"?client_id={Uri.EscapeDataString(_clientId)}" +
                          $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                          $"&response_type=code" +
                          $"&scope=openid profile email" +
                          $"&state={state}" +
                          $"&nonce={nonce}";

            return loginUrl;
        }

        public async Task<string?> HandleCallbackAsync(string code)
        {
            try
            {
                // Scambia il codice con un token
                var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";

                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUri)
                });

                var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequest);

                if (!tokenResponse.IsSuccessStatusCode)
                    return null;

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);

                if (!tokenData.TryGetProperty("access_token", out var accessTokenElement))
                    return null;

                var accessToken = accessTokenElement.GetString();

                // Salva il token
                await _cookieService.PutAsync("access_token", accessToken);

                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<UserDto?> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var userInfoEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/userinfo";

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var userResponse = await _httpClient.GetAsync(userInfoEndpoint);

                if (!userResponse.IsSuccessStatusCode)
                    return null;

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(userContent);

                // Mappa i dati dell'utente al tuo UserDto
                var userDto = new UserDto
                {
                    UserName = userData.TryGetProperty("preferred_username", out var username) ? username.GetString() : "",
                    Email = userData.TryGetProperty("email", out var email) ? email.GetString() : "",
                    Name = userData.TryGetProperty("given_name", out var firstName) ? firstName.GetString() : "",
                    Surname = userData.TryGetProperty("family_name", out var lastName) ? lastName.GetString() : ""
                };

                return userDto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetLogoutUrl()
        {
            var logoutUrl = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/logout" +
                           $"?client_id={Uri.EscapeDataString(_clientId)}" +
                           $"&post_logout_redirect_uri={Uri.EscapeDataString("https://localhost:7189/")}";

            return logoutUrl;
        }
    }
}