using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages.Authentication;

public partial class Login
{
    private LoginDto loginModel = new();
    private string? errorMessage;

    private async Task HandleLogin()
    {
        errorMessage = null;
        var token = await AuthService.LoginAsync(loginModel);
        if (token != null)
        {
            await CustomAuthStateProvider.Login(token);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            errorMessage = "Invalid username or password.";
        }
    }
}
