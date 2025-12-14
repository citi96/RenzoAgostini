using System.Net.Http.Headers;
using Blazored.LocalStorage;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Http.Handlers;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath.Contains("/api/auth/") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            var tokenDto = await _localStorage.GetItemAsync<TokenDto>("authToken", cancellationToken);
            if (tokenDto != null && !string.IsNullOrWhiteSpace(tokenDto.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
            }
        }
        catch { /* LocalStorage might fail if not available/prerendering */ }

        return await base.SendAsync(request, cancellationToken);
    }
}
