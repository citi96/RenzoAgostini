using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;

namespace RenzoAgostini.Client.Pages
{
    public partial class Checkout : ComponentBase, IDisposable
    {
        [Inject] private ICartService CartService { get; set; } = default!;
        [Inject] private IOrderService OrderService { get; set; } = default!;
        [Inject] private ICheckoutService CheckoutService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ILogger<Checkout> Logger { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        protected OrderForm orderForm = new();
        protected List<PaintingDto> cartItems = new();
        protected int currentStep = 1;
        protected bool isLoading = true;
        protected bool isProcessing = false;

        protected override async Task OnInitializedAsync()
        {
            CartService.OnChange += HandleCartChanged;
            await CartService.InitializeAsync();
            await LoadCheckoutData();
        }

        public void Dispose()
        {
            CartService.OnChange -= HandleCartChanged;
        }

        private async Task LoadCheckoutData()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Load cart items
                cartItems = [.. (await CartService.GetItemsAsync())];

                if (cartItems.Count == 0)
                {
                    Navigation.NavigateTo("/cart");
                    return;
                }

                // Pre-fill user data if authenticated
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    orderForm.Email = authState.User.Identity.Name ?? "";

                    // Extract first/last name from claims if available
                    var givenName = authState.User.FindFirst("given_name")?.Value;
                    var familyName = authState.User.FindFirst("family_name")?.Value;

                    if (!string.IsNullOrEmpty(givenName))
                        orderForm.FirstName = givenName;
                    if (!string.IsNullOrEmpty(familyName))
                        orderForm.LastName = familyName;
                }

                // Default country
                orderForm.Address.Country = "Italy";

                Logger.LogInformation("Checkout loaded with {Count} items for user {Email}",
                    cartItems.Count, orderForm.Email);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading checkout data");
                await ShowErrorToast("Errore nel caricamento del checkout");
                Navigation.NavigateTo("/cart");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private void HandleCartChanged()
        {
            InvokeAsync(async () =>
            {
                cartItems = [.. (await CartService.GetItemsAsync())];
                if (cartItems.Count == 0)
                {
                    Navigation.NavigateTo("/cart");
                    return;
                }
                StateHasChanged();
            });
        }

        protected async Task ProcessOrder()
        {
            if (!IsFormValid() || isProcessing || cartItems.Count == 0)
                return;

            try
            {
                isProcessing = true;
                StateHasChanged();

                CartService.CheckoutData = orderForm.ToCheckoutDto(cartItems.Select(p => p.Id));

                var session = await CheckoutService.CreateCheckoutSessionAsync(CartService.CheckoutData!);
                Navigation.NavigateTo(session.SessionUrl, forceLoad: true);                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing order for user {Email}", orderForm.Email);
                await ShowErrorToast("Errore durante l'elaborazione dell'ordine. Riprova.");
            }
            finally
            {
                isProcessing = false;
                StateHasChanged();
            }
        }

        // Form validation
        protected bool IsFormValid()
        {
            return !string.IsNullOrWhiteSpace(orderForm.FirstName) &&
                   !string.IsNullOrWhiteSpace(orderForm.LastName) &&
                   !string.IsNullOrWhiteSpace(orderForm.Email) &&
                   IsValidEmail(orderForm.Email) &&
                   !string.IsNullOrWhiteSpace(orderForm.Address.Street) &&
                   !string.IsNullOrWhiteSpace(orderForm.Address.City) &&
                   !string.IsNullOrWhiteSpace(orderForm.Address.PostalCode) &&
                   !string.IsNullOrWhiteSpace(orderForm.Address.Country) &&
                   orderForm.AcceptTerms;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Calculation methods
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
    }

    // Form models
    public class OrderForm
    {
        [Required(ErrorMessage = "Nome obbligatorio")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cognome obbligatorio")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email obbligatoria")]
        [EmailAddress(ErrorMessage = "Email non valida")]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [Required]
        public AddressForm Address { get; set; } = new();

        [Required(ErrorMessage = "Devi accettare i termini e condizioni")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare i termini e condizioni")]
        public bool AcceptTerms { get; set; }

        public CheckoutDto ToCheckoutDto(IEnumerable<int> paintingIds)
        {
            return new CheckoutDto(AcceptTerms)
            {
                PaintingIds = paintingIds,
                CustomerFirstName = FirstName,
                CustomerLastName = LastName,
                CustomerEmail = Email,
                AddressLine = Address.Street,
                City = Address.City,
                PostalCode = Address.PostalCode,
                Country = Address.Country,
                ShippingMethod = "Standard"
            };
        }
    }

    public class AddressForm
    {
        [Required(ErrorMessage = "Indirizzo obbligatorio")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Città obbligatoria")]
        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "CAP obbligatorio")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Paese obbligatorio")]
        public string Country { get; set; } = string.Empty;
    }
}