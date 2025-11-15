using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;
using System.Linq;

namespace RenzoAgostini.Client.Pages;

public partial class Home : ComponentBase
{
    [Inject] private IPaintingService PaintingService { get; set; } = default!;
    [Inject] private ILogger<Home> Logger { get; set; } = default!;

    protected IReadOnlyList<PaintingDto>? Paintings { get; private set; }
    protected bool IsLoading { get; private set; } = true;
    protected string? ErrorMessage { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadPaintingsAsync();
    }

    protected async Task ReloadPaintings()
    {
        await LoadPaintingsAsync();
    }

    private async Task LoadPaintingsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            await InvokeAsync(StateHasChanged);

            var result = await PaintingService.GetAllPaintingsAsync();

            Paintings = result?
                .OrderByDescending(p => p.Year)
                .ThenBy(p => p.Title)
                .ToList();

            Logger.LogInformation("Loaded {Count} paintings for gallery", Paintings?.Count ?? 0);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading paintings on home page");
            ErrorMessage = "Si è verificato un problema durante il caricamento dei quadri. Riprova più tardi.";
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
