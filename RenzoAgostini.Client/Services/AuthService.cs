using System.Net.Http.Json;
using RenzoAgostini.Shared.DTOs;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Services;



public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _anonClient;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _anonClient = _httpClientFactory.CreateClient("AuthClient");
    }

    public async Task<TokenDto?> LoginAsync(LoginDto loginDto)
    {
        var response = await _anonClient.PostAsJsonAsync("api/auth/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }
        return null;
    }

    public async Task<bool> RegisterAsync(RegisterDto registerDto)
    {
        var response = await _anonClient.PostAsJsonAsync("api/auth/register", registerDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<TokenDto?> RefreshTokenAsync(TokenDto tokenDto)
    {
        var response = await _anonClient.PostAsJsonAsync("api/auth/refresh", tokenDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }
        return null;
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        // Use authenticated client
        var salesClient = _httpClientFactory.CreateClient("ApiClient");
        var response = await salesClient.GetAsync("api/auth/users");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }
        return new List<UserDto>();
    }

    public async Task<bool> AssignRoleAsync(string username, string role)
    {
        // Use authenticated client
        var salesClient = _httpClientFactory.CreateClient("ApiClient");
        var response = await salesClient.PostAsJsonAsync("api/auth/roles", new SetRoleDto { UserName = username, Role = role });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var response = await _anonClient.PostAsJsonAsync("api/auth/forgot-password", new ForgotPasswordDto { Email = email });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var response = await _anonClient.PostAsJsonAsync("api/auth/reset-password", resetPasswordDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserProfileDto?> GetProfileAsync()
    {
        var salesClient = _httpClientFactory.CreateClient("ApiClient");
        var response = await salesClient.GetAsync("api/auth/profile");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }
        return null;
    }

    public async Task<AuthResponseDto> UpdateProfileAsync(UserProfileDto dto)
    {
        var salesClient = _httpClientFactory.CreateClient("ApiClient");
        var response = await salesClient.PostAsJsonAsync("api/auth/profile", dto);

        // Try to read generic status message
        try
        {
            if (response.IsSuccessStatusCode)
            {
                return new AuthResponseDto { IsSuccess = true };
            }
            // Read error message
            var error = await response.Content.ReadAsStringAsync();
            return new AuthResponseDto { IsSuccess = false, ErrorMessage = error };
        }
        catch
        {
            return new AuthResponseDto { IsSuccess = false, ErrorMessage = "Errore durante l'aggiornamento." };
        }
    }
}
