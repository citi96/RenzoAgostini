using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RenzoAgostini.Client.Pages
{
    public partial class Checkout : ComponentBase, IDisposable
    {
        [Inject] private ICartService CartService { get; set; } = default!;
        [Inject] private IOrderService OrderService { get; set; } = default!;
        [Inject] private ICheckoutService CheckoutService { get; set; } = default!;
        [Inject] private IShippingClient ShippingClient { get; set; } = default!;
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
        protected List<ShippingOptionDto> shippingOptions = new();
        protected ShippingOptionDto? selectedShippingOption;
        protected bool isLoadingShipping = true;
        protected string? shippingError;

        protected string SelectedCountry
        {
            get => orderForm.Address.Country;
            set
            {
                if (orderForm.Address.Country != value)
                {
                    orderForm.Address.Country = value;
                    _ = ReloadShippingOptionsAsync(value);
                }
            }
        }

        private Task ReloadShippingOptionsAsync(string country)
        {
            return InvokeAsync(() => LoadShippingOptionsAsync(country));
        }

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
                orderForm.ShippingOptionId = CartService.SelectedShippingOption?.Id;

                await LoadShippingOptionsAsync(orderForm.Address.Country);

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

        private async Task LoadShippingOptionsAsync(string country)
        {
            try
            {
                isLoadingShipping = true;
                shippingError = null;
                StateHasChanged();

                var options = await ShippingClient.GetOptionsAsync(country);
                shippingOptions = [.. options];

                if (shippingOptions.Count == 0)
                {
                    selectedShippingOption = null;
                    orderForm.ShippingOptionId = null;
                    CartService.SetShippingOption(null);
                    return;
                }

                var preferredId = CartService.SelectedShippingOption?.Id ?? orderForm.ShippingOptionId;
                selectedShippingOption = preferredId.HasValue
                    ? shippingOptions.FirstOrDefault(o => o.Id == preferredId.Value)
                    : null;

                if (selectedShippingOption is null)
                {
                    selectedShippingOption = shippingOptions.First();
                }

                ApplyShippingSelection(selectedShippingOption);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading shipping options for {Country}", country);
                shippingOptions = [];
                selectedShippingOption = null;
                orderForm.ShippingOptionId = null;
                CartService.SetShippingOption(null);
                shippingError = "Impossibile caricare le opzioni di spedizione per la destinazione selezionata.";
            }
            finally
            {
                isLoadingShipping = false;
                StateHasChanged();
            }
        }

        private void ApplyShippingSelection(ShippingOptionDto option)
        {
            selectedShippingOption = option;
            orderForm.ShippingOptionId = option.Id;
            shippingError = null;
            CartService.SetShippingOption(option);
        }

        protected void SelectShippingOption(ShippingOptionDto option)
        {
            ApplyShippingSelection(option);
            StateHasChanged();
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
                   selectedShippingOption is not null &&
                   shippingError is null &&
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
            if (selectedShippingOption is null)
            {
                return 0;
            }

            if (selectedShippingOption.IsPickup)
            {
                return 0;
            }

            var subtotal = GetSubtotal();
            if (selectedShippingOption.FreeShippingThreshold is decimal threshold && subtotal >= threshold)
            {
                return 0;
            }

            return selectedShippingOption.Cost;
        }

        protected decimal GetTotal()
        {
            return GetSubtotal() + GetShipping();
        }

        private decimal CalculateShippingCost(ShippingOptionDto option)
        {
            if (option.IsPickup)
            {
                return 0;
            }

            var subtotal = GetSubtotal();
            if (option.FreeShippingThreshold is decimal threshold && subtotal >= threshold)
            {
                return 0;
            }

            return option.Cost;
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
        public int? ShippingOptionId { get; set; }

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
                ShippingOptionId = ShippingOptionId
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