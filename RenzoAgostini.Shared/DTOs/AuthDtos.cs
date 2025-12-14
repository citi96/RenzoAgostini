using System.ComponentModel.DataAnnotations;

namespace RenzoAgostini.Shared.DTOs;

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}

public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class SetRoleDto
{
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
