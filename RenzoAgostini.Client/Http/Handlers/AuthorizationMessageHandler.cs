using System.Net.Http.Headers;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Http.Handlers
{
    public class AuthorizationMessageHandler(ICookieService cookieService) : DelegatingHandler()
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await cookieService.GetAsync<string>("access_token");
            if (!string.IsNullOrEmpty(token) && request.Headers.Authorization == null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
