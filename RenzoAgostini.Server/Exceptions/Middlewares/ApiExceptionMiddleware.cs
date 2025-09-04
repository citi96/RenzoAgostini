using System.Net;

namespace RenzoAgostini.Server.Exceptions.Middlewares
{
    public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Handled API Exception: {ex.StatusCode}\t-\t{ex.Message}");

                context.Response.StatusCode = (int)ex.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unhandled Exception: {HttpStatusCode.InternalServerError}\t-\t{ex.Message}");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
