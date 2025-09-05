using RenzoAgostini.Client.Http.Handlers;

namespace RenzoAgostini.Client.Http
{
    public class CustomHttpClient : HttpClient
    {
        public new Uri? BaseAddress => base.BaseAddress;

        public CustomHttpClient(AuthorizationMessageHandler authorizationHandler, ErrorMessageHandler errorMessageHandler) : base(authorizationHandler)
        {
            base.BaseAddress = new Uri("https://localhost:7215");

            authorizationHandler.InnerHandler = errorMessageHandler;
            errorMessageHandler.InnerHandler = new HttpClientHandler();
        }
    }
}
