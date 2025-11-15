using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class Cart : ComponentBase, IDisposable
    {
        [Inject] private ICartService CartService { get; set; } = default!;
        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ILogger<Cart> Logger { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        protected List<PaintingDto> cartItems = [];
        protected List<PaintingDto>? recommendedPaintings;
        protected bool isLoading = true;
        protected bool isUpdating = false;

        protected override async Task OnInitializedAsync()
        {
            CartService.OnChange += HandleCartChanged;

            await CartService.InitializeAsync();
            await LoadCartItems();
            await LoadRecommendedPaintings();

            isLoading = false;
        }

        public void Dispose()
        {
            CartService.OnChange -= HandleCartChanged;
        }

        private async Task LoadCartItems()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                var items = await CartService.GetItemsAsync();
                cartItems = items.ToList();

                Logger.LogInformation("Loaded {Count} items in cart", cartItems.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading cart items");
                await ShowErrorToast("Errore nello svuotare il carrello");
            }
            finally
            {
                isUpdating = false;
                StateHasChanged();
            }
        }

        protected void GoToCheckout()
        {
            if (!cartItems.Any() || isUpdating) return;

            try
            {
                Navigation.NavigateTo("/checkout");
                Logger.LogInformation("User navigated to checkout with {Count} items", cartItems.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error navigating to checkout");
                ShowErrorToast("Errore nel reindirizzamento al checkout").GetAwaiter().GetResult();
            }
        }

        // Helper methods for calculations
        protected int GetTotalItems()
        {
            return cartItems.Count;
        }

        protected decimal GetSubtotal()
        {
            return cartItems.Sum(item => (item.Price ?? 0));
        }

        protected decimal GetShipping()
        {
            return GetSubtotal() >= 200 ? 0 : 15;
        }

        protected decimal GetTotal()
        {
            return GetSubtotal() + GetShipping();
        }

        // Toast notifications
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

        private async Task LoadRecommendedPaintings()
        {
            try
            {
                var allPaintings = await PaintingService.GetAllPaintingsAsync();

                // Filter out items already in cart and get random recommendations
                var availablePaintings = allPaintings
                    .Where(p => p.IsForSale && !cartItems.Any(ci => ci.Id == p.Id))
                    .ToList();

                // Simple random selection - in real app you'd use ML recommendations
                var random = new Random();
                recommendedPaintings = [.. availablePaintings
                    .OrderBy(x => random.Next())
                    .Take(6)];

                StateHasChanged();

                Logger.LogInformation("Loaded {Count} recommended paintings", recommendedPaintings.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading recommended paintings");
                // Non-critical error, don't show toast
            }
        }

        private void HandleCartChanged()
        {
            InvokeAsync(async () =>
            {
                await LoadCartItems();
                await LoadRecommendedPaintings(); // Refresh recommendations
            });
        }

        protected async Task RemoveItem(int paintingId)
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;
                StateHasChanged();

                var item = cartItems.FirstOrDefault(i => i.Id == paintingId);
                if (item == null) return;

                await CartService.RemoveItemAsync(paintingId);
                await ShowSuccessToast($"'{item.Title}' rimosso dal carrello");

                Logger.LogInformation("Removed painting {PaintingId} from cart", paintingId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error removing item {PaintingId} from cart", paintingId);
                await ShowErrorToast("Errore nella rimozione dell'articolo");
            }
            finally
            {
                isUpdating = false;
                StateHasChanged();
            }
        }

        protected async Task ClearCart()
        {
            if (isUpdating || cartItems.Count == 0) return;

            // Confirm with user
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                "Sei sicuro di voler svuotare completamente il carrello?");

            if (!confirmed) return;

            try
            {
                isUpdating = true;
                StateHasChanged();

                await CartService.ClearAsync();
                await ShowSuccessToast("Carrello svuotato con successo");

                Logger.LogInformation("Cart cleared by user");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error clearing cart");
                await ShowErrorToast("Errore nel caricamento del carrello");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }
}