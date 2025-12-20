using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces;

public interface IAuthService
{
    Task<TokenDto?> LoginAsync(LoginDto loginDto);
    Task<bool> RegisterAsync(RegisterDto registerDto);
    Task<TokenDto?> RefreshTokenAsync(TokenDto tokenDto);
    Task<List<UserDto>> GetUsersAsync();
    Task<bool> AssignRoleAsync(string username, string role);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<UserProfileDto?> GetProfileAsync();
    Task<AuthResponseDto> UpdateProfileAsync(UserProfileDto dto);
}
