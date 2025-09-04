using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class UserService(HttpClient httpClient, ICookieService cookieService) : IUserService
    {
        public async Task<UserDto?> SendAuthenticateRequestAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/login");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                await cookieService.PutAsync("access_token", token);

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
            var token = await cookieService.GetAsync<string>("access_token");
            var claimsPrincipal = CreateClaimsPrincipalFromToken(token);
            var userDto = UserDto.FromClaimsPrincipal(claimsPrincipal);

            return userDto;
        }
    }
}
