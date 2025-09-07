using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Components
{
    public partial class PaintingCard : ComponentBase
    {
        [Inject] IConfiguration Configuration { get; set; } = default!;
        [Inject] ICartService CartService { get; set; } = default!;

        [Parameter] public required string Title { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public int? Year { get; set; }
        [Parameter] public string? Medium { get; set; }
        [Parameter] public decimal? Price { get; set; }
        [Parameter] public bool IsForSale { get; set; }
        [Parameter] public IReadOnlyList<string> ImageUrls { get; set; } = [];
        [Parameter] public bool IsAdminMode { get; set; } = false;
        [Parameter] public EventCallback<PaintingDto> OnEdit { get; set; }
        [Parameter] public EventCallback<PaintingDto> OnDelete { get; set; }
        [Parameter] public PaintingDto? PaintingDto { get; set; }

        protected string PrimaryImageUrl => ImageUrls.Count > 0 ? ImageUrls[0] : string.Empty;

        private bool showLightbox = false;
        private int lightboxIndex = 0;

        private void ShowLightbox(int index)
        {
            lightboxIndex = index;
            showLightbox = true;
        }

        private void HideLightbox()
        {
            showLightbox = false;
        }

        private void AddToCart()
        {
            if (PaintingDto is not null)
            {
                CartService.AddItem(PaintingDto);
            }
        }
    }
}