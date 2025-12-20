using System.ComponentModel.DataAnnotations;

namespace RenzoAgostini.Shared.DTOs;

public class UserProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Read-only typically, but good for display

    [Required(ErrorMessage = "La password attuale è necessaria per salvare le modifiche.")]
    public string CurrentPassword { get; set; } = string.Empty;

    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "Le password non coincidono.")]
    public string? ConfirmNewPassword { get; set; }
}
