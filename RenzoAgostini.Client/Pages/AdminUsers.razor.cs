using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages;

public partial class AdminUsers
{
    private List<UserDto>? users;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        users = await AuthService.GetUsersAsync();
    }

    private async Task SetRole(string username, string role)
    {
        var result = await AuthService.AssignRoleAsync(username, role);
        if (result)
        {
            await LoadUsers();
        }
    }
}
