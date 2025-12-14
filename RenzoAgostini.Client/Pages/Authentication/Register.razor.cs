using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages.Authentication;

public partial class Register
{
    private RegisterDto registerModel = new();
    private string? errorMessage;

    private async Task HandleRegister()
    {
        errorMessage = null;
        var success = await AuthService.RegisterAsync(registerModel);
        if (success)
        {
            NavigationManager.NavigateTo("authentication/login");
        }
        else
        {
            errorMessage = "Registration failed. User might already exist.";
        }
    }
}
