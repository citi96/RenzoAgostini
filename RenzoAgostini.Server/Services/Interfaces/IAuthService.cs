using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser> ValidateUserCredentialsAsync(string token);
        Task CreateUserAsync(ApplicationUser user);
        Task<ApplicationUser> GetApplicationUserAsync(string token);
    }
}
