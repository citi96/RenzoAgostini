using Microsoft.Extensions.Configuration;
using RenzoAgostini.Client.Http.Handlers;

namespace RenzoAgostini.Client.Http
{
    public class CustomHttpClient : HttpClient
    {
        public new Uri? BaseAddress => base.BaseAddress;

        public CustomHttpClient(
            AuthorizationMessageHandler authorizationHandler,
            RefreshTokenHandler refreshTokenHandler,
            ErrorMessageHandler errorMessageHandler,
            IConfiguration configuration) : base(authorizationHandler)
        {
            var baseUrl = configuration["BaseUrl"];
            base.BaseAddress = !string.IsNullOrWhiteSpace(baseUrl)
                ? new Uri(baseUrl)
                : new Uri("https://localhost:7215");

            authorizationHandler.InnerHandler = refreshTokenHandler;
            refreshTokenHandler.InnerHandler = errorMessageHandler;
            errorMessageHandler.InnerHandler = new HttpClientHandler();
        }
    }
}
