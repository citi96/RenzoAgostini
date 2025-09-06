using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Server.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool TermsAccepted { get; set; } 
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? TrackingNumber { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
