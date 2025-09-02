using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Shared.Contracts
{
    public interface IUserService
    {
        Task<UserDto?> SendAuthenticateRequestAsync(string username, string password);
        Task<UserDto?> SendAuthenticateRequestAsync(EProvider provider, string credential);
        Task<UserDto?> FetchUserFromBrowserAsync();
    }
}
