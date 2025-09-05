namespace RenzoAgostini.Client.Http.Handlers
{
    public class ErrorMessageHandler(/*ToastService toastService*/) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null!;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                //if (!response.IsSuccessStatusCode)
                //    toastService.Notify(new(ToastType.Danger, $"{request.RequestUri}\n{response.StatusCode} - " +
                //        $"{await response.Content.ReadAsStringAsync(cancellationToken)}"));
                //else
                //    toastService.Notify(new(ToastType.Success, $"{request.RequestUri}"));

                return response;
            }
            catch (Exception ex)
            {
                //toastService.Notify(new(ToastType.Danger, ex.Message));
                return response;
            }
        }
    }
}
