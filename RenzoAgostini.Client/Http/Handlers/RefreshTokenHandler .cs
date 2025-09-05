using System.Net;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Http.Handlers
{
    public sealed class RefreshTokenHandler(IConfiguration configuration, ICookieService cookies) : DelegatingHandler
    {
        private static readonly SemaphoreSlim Gate = new(1, 1);

        private readonly Uri TokenEndpoint =
            new($"{configuration["Keycloak:Url"]}/realms/RenzoAgostiniRealm/protocol/openid-connect/token");
        private readonly string? ClientId = configuration["Keycloak:ClientId"];

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
                if (string.IsNullOrWhiteSpace(refresh) || string.IsNullOrWhiteSpace(ClientId))
                    return res;

                using var hc = new HttpClient();
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = ClientId,
                    ["refresh_token"] = refresh
                });
                using var tok = await hc.PostAsync(TokenEndpoint, form, ct);
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
    }
}