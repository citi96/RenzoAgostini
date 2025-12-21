using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;
using RenzoAgostini.Client.Authentication;

namespace RenzoAgostini.Client.Pages;

public partial class UserProfile : ComponentBase
{
    private UserProfileDto? profile;
    private bool isLoading = true;
    private string? errorMessage;
    private string? successMessage;

    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            profile = await AuthService.GetProfileAsync();
        }
        catch (Exception)
        {
            errorMessage = "Errore di connessione.";
        }
        finally
        {
            isLoading = false;
        }
    }

    protected async Task HandleSubmit()
    {
        errorMessage = null;
        successMessage = null;

        if (profile == null) return;

        var result = await AuthService.UpdateProfileAsync(profile);
        if (result.IsSuccess)
        {
            successMessage = "Profilo aggiornato con successo!";
            // Clear passwords fields
            profile.CurrentPassword = string.Empty;
            profile.NewPassword = string.Empty;
            profile.ConfirmNewPassword = string.Empty;
        }
        else
        {
            errorMessage = result.ErrorMessage ?? "Errore durante l'aggiornamento.";
        }
    }

    protected async Task DeleteAccount()
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Sei sicuro di voler eliminare definitivamente il tuo account? Questa azione è irreversibile.");
        if (!confirmed) return;

        var success = await AuthService.DeleteAccountAsync();
        if (success)
        {
            await AuthStateProvider.Logout();
            Navigation.NavigateTo("/", forceLoad: true);
        }
        else
        {
            errorMessage = "Impossibile eliminare l'account in questo momento.";
        }
    }
}
