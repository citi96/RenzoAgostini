﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Components
{
    public partial class PaintingCard : ComponentBase
    {
        [Inject] IConfiguration Configuration { get; set; } = default!;
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;
        [Inject] ILogger<PaintingCard> Logger { get; set; } = default!;

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
        [Parameter] public bool IsLoading { get; set; } = false;

        protected string PrimaryImageUrl => ImageUrls.Count > 0 ? ImageUrls[0] : string.Empty;

        private bool showLightbox = false;
        private int lightboxIndex = 0;
        private bool isAddingToCart = false;
        private bool isFavorite = false; // TODO: Implementare con localStorage o servizio

        protected bool IsAddingToCart => isAddingToCart;
        protected bool IsFavorite => isFavorite;

        protected void ShowLightbox(int index)
        {
            lightboxIndex = index;
            showLightbox = true;
        }

        protected void HideLightbox()
        {
            showLightbox = false;
        }

        protected async Task AddToCart()
        {
            if (PaintingDto is null || !Price.HasValue || isAddingToCart) return;

            try
            {
                isAddingToCart = true;
                StateHasChanged();

                // Simula un piccolo delay per UX migliore
                await Task.Delay(500);

                CartService.AddItem(PaintingDto);

                // Mostra notifica di successo
                await ShowSuccessToast("Quadro aggiunto al carrello!");

                Logger.LogInformation("Painting {PaintingId} added to cart", PaintingDto.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error adding painting {PaintingId} to cart", PaintingDto?.Id);
                await ShowErrorToast("Errore durante l'aggiunta al carrello");
            }
            finally
            {
                isAddingToCart = false;
                StateHasChanged();
            }
        }

        protected async Task ToggleFavorite()
        {
            try
            {
                isFavorite = !isFavorite;
                StateHasChanged();

                // TODO: Salvare lo stato dei preferiti in localStorage o servizio
                if (isFavorite)
                {
                    await ShowInfoToast("Aggiunto ai preferiti");
                }
                else
                {
                    await ShowInfoToast("Rimosso dai preferiti");
                }

                Logger.LogInformation("Painting {PaintingId} favorite toggled to {IsFavorite}",
                    PaintingDto?.Id, isFavorite);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error toggling favorite for painting {PaintingId}", PaintingDto?.Id);
                // Revert state
                isFavorite = !isFavorite;
                StateHasChanged();
            }
        }

        protected async Task SharePainting()
        {
            if (PaintingDto is null) return;

            try
            {
                var shareUrl = $"{Navigation.BaseUri}quadro/{PaintingDto.Id}";
                var shareText = $"Guarda questo bellissimo quadro: {Title}";

                // Prova a usare l'API Web Share se disponibile
                var shared = await JSRuntime.InvokeAsync<bool>("shareContent", new
                {
                    title = Title,
                    text = shareText,
                    url = shareUrl
                });

                if (!shared)
                {
                    // Fallback: copia URL negli appunti
                    await JSRuntime.InvokeVoidAsync("copyToClipboard", shareUrl);
                    await ShowSuccessToast("Link copiato negli appunti!");
                }

                Logger.LogInformation("Painting {PaintingId} shared", PaintingDto.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sharing painting {PaintingId}", PaintingDto?.Id);
                await ShowErrorToast("Errore durante la condivisione");
            }
        }

        protected void HandleCardClick()
        {
            if (IsAdminMode || isAddingToCart) return;

            try
            {
                // Naviga alla pagina di dettaglio del quadro
                if (PaintingDto?.Id != null)
                {
                    Navigation.NavigateTo($"/quadro/{PaintingDto.Id}");
                }
                else
                {
                    // Se non c'è ID, mostra la lightbox
                    if (ImageUrls.Any())
                    {
                        ShowLightbox(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error navigating to painting detail for {PaintingId}", PaintingDto?.Id);
            }
        }

        // Helper methods per toast notifications
        private async Task ShowSuccessToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "success");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing success toast");
            }
        }

        private async Task ShowErrorToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "error");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing error toast");
            }
        }

        private async Task ShowInfoToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "info");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing info toast");
            }
        }
    }
}