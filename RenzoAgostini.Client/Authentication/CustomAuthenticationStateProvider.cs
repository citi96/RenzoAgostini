using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenDto = await _localStorage.GetItemAsync<TokenDto>("authToken");

            if (tokenDto == null || string.IsNullOrWhiteSpace(tokenDto.AccessToken))
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

            var claimsPrincipal = GetClaimsPrincipalFromToken(tokenDto.AccessToken);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task Login(TokenDto tokenDto)
    {
        await _localStorage.SetItemAsync("authToken", tokenDto);
        var authState = Task.FromResult(new AuthenticationState(GetClaimsPrincipalFromToken(tokenDto.AccessToken)));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    private ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var normalizedClaims = jwtToken.Claims
            .Select(claim => claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase)
                ? new Claim("role", claim.Value.ToLowerInvariant())
                : claim)
            .ToList();

        var identity = new ClaimsIdentity(
            normalizedClaims,
            authenticationType: "JwtAuth",
            nameType: ClaimTypes.Name,
            roleType: "role");

        return new ClaimsPrincipal(identity);
    }
}