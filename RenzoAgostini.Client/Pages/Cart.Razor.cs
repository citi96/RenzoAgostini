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
        [Inject] NavigationManager Navigation { get; set; } = default!;
        [Inject] CustomAuthenticationStateProvider AuthProvider { get; set; } = default!;

        private readonly CheckoutFormModel _checkoutModel = new();

        protected override async Task OnInitializedAsync()
        {
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

            CartService.OnChange += StateHasChanged;
        }

        private void RemoveFromCart(int paintingId)
        {
            CartService.RemoveItem(paintingId);
        }

        private void ProceedToCheckout()
        {
            CartService.CheckoutData = _checkoutModel.ToCheckoutDto([.. CartService.Items.Select(p => p.Id)]);
            Navigation.NavigateTo("/checkout");
        }

        public void Dispose()
        {
            CartService.OnChange -= StateHasChanged;
            GC.SuppressFinalize(this);
        }

        // Modello del form con validazione
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
            [Required(ErrorMessage = "Nazione richiesta")]
            public string Country { get; set; } = string.Empty;
            [Required(ErrorMessage = "Seleziona un metodo di spedizione")]
            public string ShippingMethod { get; set; } = string.Empty;
            public bool TermsAccepted { get; set; } = false;

            // Validazione custom: TermsAccepted deve essere true
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!TermsAccepted)
                {
                    yield return new ValidationResult(
                        "Devi accettare i termini e condizioni per procedere.",
                        new[] { nameof(TermsAccepted) });
                }
            }

            // Conversione in CheckoutDto per invio al server
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
                    // Nota: CheckoutDto non ha un campo ShippingMethod, ma lo gestiremo lato server con parametri noti.
                };
            }
        }
    }}
