using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class Cart : ComponentBase, IDisposable
    {
        [Inject] ICartService CartService { get; set; } = default!;
        [Inject] ICheckoutClient CheckoutClient { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;
        [Inject] CustomAuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] IConfiguration Configuration { get; set; } = default!;

        private readonly CheckoutFormModel _checkoutModel = new();
        private string selectedShipping = "";
        private string? errorMessage;
        private bool showTermsModal = false;

        protected override async Task OnInitializedAsync()
        {
            // Pre-popola i dati dell'utente se autenticato
            var state = await AuthProvider.GetAuthenticationStateAsync();
            if (state.User.Identity?.IsAuthenticated == true)
            {
                var user = state.User;
                _checkoutModel.FirstName = user.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value
                                          ?? user.Identity.Name?.Split(' ')?.FirstOrDefault() ?? "";
                _checkoutModel.LastName = user.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value
                                         ?? user.Identity.Name?.Split(' ')?.LastOrDefault() ?? "";
                _checkoutModel.Email = user.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
            }

            // Imposta spedizione standard come default
            _checkoutModel.ShippingMethod = "Standard";
            selectedShipping = "Standard";

            CartService.OnChange += StateHasChanged;
        }

        private void RemoveFromCart(int paintingId)
        {
            CartService.RemoveItem(paintingId);
        }

        private void NavigateToGallery()
        {
            Navigation.NavigateTo("/");
        }

        private async Task OnCountryChange(ChangeEventArgs e)
        {
            _checkoutModel.Country = e.Value?.ToString() ?? "";
            // Reset shipping method quando cambia il paese
            _checkoutModel.ShippingMethod = "";
            selectedShipping = "";
            StateHasChanged();
        }

        private void SelectShipping(string method)
        {
            _checkoutModel.ShippingMethod = method;
            selectedShipping = method;
            StateHasChanged();
        }

        private decimal GetShippingCost(string method)
        {
            if (string.IsNullOrEmpty(_checkoutModel.Country)) return 0m;

            bool isDomestic = _checkoutModel.Country.ToLower().Contains("italia");

            return method.ToLower() switch
            {
                "express" => isDomestic ? 20m : 50m,
                "standard" => isDomestic ? 10m : 25m,
                _ => 0m
            };
        }

        private decimal GetTotalWithShipping()
        {
            var shippingCost = !string.IsNullOrEmpty(_checkoutModel.ShippingMethod)
                ? GetShippingCost(_checkoutModel.ShippingMethod)
                : 0m;
            return CartService.TotalAmount + shippingCost;
        }

        private bool CanProceed()
        {
            return !string.IsNullOrWhiteSpace(_checkoutModel.FirstName) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.LastName) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.Email) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.AddressLine) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.City) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.PostalCode) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.Country) &&
                   !string.IsNullOrWhiteSpace(_checkoutModel.ShippingMethod) &&
                   _checkoutModel.TermsAccepted &&
                   CartService.ItemsCount > 0;
        }

        private void ShowTerms()
        {
            showTermsModal = true;
        }

        private void HideTerms()
        {
            showTermsModal = false;
        }

        private void AcceptTerms()
        {
            _checkoutModel.TermsAccepted = true;
            showTermsModal = false;
            StateHasChanged();
        }

        private async Task ProceedToPayment()
        {
            try
            {
                errorMessage = null;

                // Crea il CheckoutDto con tutti i dati
                var checkoutDto = _checkoutModel.ToCheckoutDto([.. CartService.Items.Select(p => p.Id)]);

                // Chiama direttamente Stripe per creare la sessione
                var session = await CheckoutClient.CreateCheckoutSessionAsync(checkoutDto);

                // Reindirizza direttamente a Stripe
                Navigation.NavigateTo(session.SessionUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                errorMessage = "Si è verificato un errore durante la creazione della sessione di pagamento. Riprova.";
                Console.Error.WriteLine("Errore checkout: " + ex.Message);
            }
        }

        public void Dispose()
        {
            CartService.OnChange -= StateHasChanged;
            GC.SuppressFinalize(this);
        }

        // Modello del form con validazione (stesso di prima)
        public class CheckoutFormModel : IValidatableObject
        {
            [Required(ErrorMessage = "Nome richiesto")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Cognome richiesto")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email richiesta"), EmailAddress(ErrorMessage = "Email non valida")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Indirizzo richiesto")]
            public string AddressLine { get; set; } = string.Empty;

            [Required(ErrorMessage = "Città richiesta")]
            public string City { get; set; } = string.Empty;

            [Required(ErrorMessage = "CAP richiesto")]
            public string PostalCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Paese richiesto")]
            public string Country { get; set; } = string.Empty;

            [Required(ErrorMessage = "Seleziona un metodo di spedizione")]
            public string ShippingMethod { get; set; } = string.Empty;

            public bool TermsAccepted { get; set; } = false;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!TermsAccepted)
                {
                    yield return new ValidationResult(
                        "Devi accettare i termini e condizioni per procedere.",
                        new[] { nameof(TermsAccepted) });
                }
            }

            public CheckoutDto ToCheckoutDto(List<int> paintingIds)
            {
                return new CheckoutDto(TermsAccepted)
                {
                    PaintingIds = paintingIds,
                    CustomerFirstName = FirstName,
                    CustomerLastName = LastName,
                    CustomerEmail = Email,
                    AddressLine = AddressLine,
                    City = City,
                    PostalCode = PostalCode,
                    Country = Country,
                    ShippingMethod = ShippingMethod // Ora lo passiamo anche nel DTO
                };
            }
        }
    }
}