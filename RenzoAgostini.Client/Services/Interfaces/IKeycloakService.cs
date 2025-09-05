using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface IKeycloakService
    {
        Task<string> GetLoginUrlAsync();
        Task<string?> HandleCallbackAsync(string code);
        string GetLogoutUrl();
    }
}
