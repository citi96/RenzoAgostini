using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class UserService(HttpClient httpClient, ICookieService cookieService) : IUserService
    {
        public async Task<UserDto?> SendAuthenticateRequestAsync(string username, string password)
        {
            var response = await httpClient.GetAsync($"/example-data/{username}.json");

            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync();
                var claimPrincipal = CreateClaimsPrincipalFromToken(token);
                var user = UserDto.FromClaimsPrincipal(claimPrincipal);

                await cookieService.PutAsync("token", token);

                return user;
            }

            return null;
        }

        public async Task<UserDto?> SendAuthenticateRequestAsync(EProvider provider, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/login/{provider}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                await cookieService.PutAsync("token", token);

                return user;
            }

            return null;
        }

        private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity();

            if (tokenHandler.CanReadToken(token))
            {
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                identity = new(jwtSecurityToken.Claims, "Bearer");
            }

            return new(identity);
        }

        public async Task<UserDto?> FetchUserFromBrowserAsync()
        {
            // TODO: Please note that this code sample does not encrypt the user's password. In a real project, you should consider adding encryption.
            var token = await cookieService.GetAsync<string>("token");
            var claimsPrincipal = CreateClaimsPrincipalFromToken(token);
            var userDto = UserDto.FromClaimsPrincipal(claimsPrincipal);

            return userDto;
        }
    }
}
