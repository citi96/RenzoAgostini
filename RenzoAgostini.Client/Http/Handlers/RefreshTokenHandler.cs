using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Http.Handlers;

public sealed class RefreshTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IAuthService _authService;

    public RefreshTokenHandler(ILocalStorageService localStorage, IAuthService authService)
    {
        _localStorage = localStorage;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var res = await base.SendAsync(req, ct);

        if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            try
            {
                var tokenDto = await _localStorage.GetItemAsync<TokenDto>("authToken", ct);
                if (tokenDto == null || string.IsNullOrWhiteSpace(tokenDto.RefreshToken))
                {
                    return res;
                }

                var newToken = await _authService.RefreshTokenAsync(tokenDto);
                if (newToken != null)
                {
                    await _localStorage.SetItemAsync("authToken", newToken, ct);

                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                    return await base.SendAsync(req, ct);
                }
                else
                {
                    await _localStorage.RemoveItemAsync("authToken", ct);
                }
            }
            catch { /* Handle errors silently, return original 401 */ }
        }

        return res;
    }
}