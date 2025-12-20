using System.Net.Http.Json;
using RenzoAgostini.Shared.DTOs;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Services;

public interface IAuthService
{
    Task<TokenDto?> LoginAsync(LoginDto loginDto);
    Task<bool> RegisterAsync(RegisterDto registerDto);
    Task<TokenDto?> RefreshTokenAsync(TokenDto tokenDto);
    Task<List<UserDto>> GetUsersAsync();
    Task<bool> AssignRoleAsync(string username, string role);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}

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
}
