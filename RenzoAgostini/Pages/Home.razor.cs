using global::RenzoAgostini.Models.DTOs;
using global::RenzoAgostini.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace RenzoAgostini.Pages
{
    public partial class Home
    {
        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private ILogger<Home> Logger { get; set; } = default!;

        protected IEnumerable<PaintingDto>? paintings;
        protected string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                paintings = await PaintingService.GetAllPaintingsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading paintings on home page");
                errorMessage = "Errore nel caricamento dei quadri. Riprova più tardi.";
                // In produzione, potresti voler mostrare un messaggio più user-friendly
            }
        }
    }
}