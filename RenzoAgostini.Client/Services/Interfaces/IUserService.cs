using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> SendAuthenticateRequestAsync(string token);
    }
}
