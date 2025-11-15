using System.Text.Json;
using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class CustomPainting : ComponentBase
    {
        [Inject] private ICookieService CookieService { get; set; } = default!;
        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private ICartService CartService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ILogger<CustomPainting> Logger { get; set; } = default!;

        protected PaintingDto? painting;
        protected string currentImage = string.Empty;
        protected bool isAddingToCart = false;
        protected bool addedToCart = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var customOrderDtoJson = await CookieService.GetAsync<string>("customOrder");
                var customOrderDto = JsonSerializer.Deserialize<CustomOrderDto>(customOrderDtoJson);
                if (customOrderDto?.PaintingId == null)
                {
                    Navigation.NavigateTo("/not-found");
                    return;
                }

                painting = await PaintingService.GetPaintingByIdAsync(customOrderDto.PaintingId.Value);
                if (painting == null)
                    return;

                if (painting.ImageUrls.Any() == true)
                    currentImage = painting.ImageUrls[0];
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading custom painting");
                Navigation.NavigateTo("/not-found");
            }
        }

        protected void SetCurrentImage(string imageUrl)
        {
            currentImage = imageUrl;
            StateHasChanged();
        }

        protected async Task AddToCart()
        {
            if (painting == null || !painting.Price.HasValue) return;

            try
            {
                isAddingToCart = true;
                StateHasChanged();

                await CartService.AddItemAsync(painting);
                addedToCart = true;
                StateHasChanged();

                // Reset success message after 3 seconds
                await Task.Delay(3000);
                addedToCart = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error adding painting {PaintingId} to cart", painting.Id);
            }
            finally
            {
                isAddingToCart = false;
                StateHasChanged();
            }
        }
    }
}