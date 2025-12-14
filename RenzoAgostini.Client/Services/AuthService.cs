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
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TokenDto?> LoginAsync(LoginDto loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }
        return null;
    }

    public async Task<bool> RegisterAsync(RegisterDto registerDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<TokenDto?> RefreshTokenAsync(TokenDto tokenDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", tokenDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TokenDto>();
        }
        return null;
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("api/auth/users");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }
        return new List<UserDto>();
    }

    public async Task<bool> AssignRoleAsync(string username, string role)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/roles", new SetRoleDto { UserName = username, Role = role });
        return response.IsSuccessStatusCode;
    }
}
