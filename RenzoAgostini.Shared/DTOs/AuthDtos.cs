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

    [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare la Privacy Policy e i Termini di servizio.")]
    public bool PrivacyAccepted { get; set; }
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

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
