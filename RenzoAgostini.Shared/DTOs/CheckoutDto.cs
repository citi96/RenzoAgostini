namespace RenzoAgostini.Shared.DTOs
{
    public record CheckoutDto(bool TermsAccepted)
    {
        public IEnumerable<int> PaintingIds { get; set; } = [];
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = "Standard";
    }
}
