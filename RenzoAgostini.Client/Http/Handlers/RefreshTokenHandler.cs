using System.Net;
using Microsoft.Extensions.Configuration;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Http.Handlers
{
    public sealed class RefreshTokenHandler(IConfiguration configuration, ICookieService cookies) : DelegatingHandler
    {
        private static readonly SemaphoreSlim Gate = new(1, 1);

        private readonly Uri? _tokenEndpoint = BuildTokenEndpoint(configuration);
        private readonly string? _clientId = configuration["Keycloak:ClientId"];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var res = await base.SendAsync(req, ct);
            if (res.StatusCode != HttpStatusCode.Unauthorized)
                return res;

            await Gate.WaitAsync(ct);
            try
            {
                var current = await cookies.GetAsync<string>("access_token");
                if (req.Headers.Authorization?.Parameter != current)
                {
                    req.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", current);
                    res.Dispose();
                    return await base.SendAsync(req, ct);
                }

                var refresh = await cookies.GetAsync<string>("refresh_token");
                if (_tokenEndpoint is null || string.IsNullOrWhiteSpace(refresh) || string.IsNullOrWhiteSpace(_clientId))
                    return res;

                using var hc = new HttpClient();
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = _clientId!,
                    ["refresh_token"] = refresh
                });
                using var tok = await hc.PostAsync(_tokenEndpoint, form, ct);
                if (!tok.IsSuccessStatusCode) 
                    return res;

                using var json = System.Text.Json.JsonDocument.Parse(await tok.Content.ReadAsStringAsync(ct));
                var newAccess = json.RootElement.GetProperty("access_token").GetString();
                var newRefresh = json.RootElement.GetProperty("refresh_token").GetString();

                await cookies.PutAsync("access_token", newAccess!);
                await cookies.PutAsync("refresh_token", newRefresh!);

                req.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAccess);
                res.Dispose();
                return await base.SendAsync(req, ct);
            }
            finally { Gate.Release(); }
        }
        private static Uri? BuildTokenEndpoint(IConfiguration configuration)
        {
            var baseUrl = configuration["Keycloak:Url"];
            var realm = configuration["Keycloak:Realm"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(realm))
                return null;

            return new Uri($"{baseUrl.TrimEnd('/')}/realms/{realm}/protocol/openid-connect/token");
        }
    }
}