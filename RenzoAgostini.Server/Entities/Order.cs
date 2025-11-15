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
        public decimal ItemsTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? TrackingNumber { get; set; }

        public int? ShippingOptionId { get; set; }
        public ShippingOption? ShippingOption { get; set; }
        public string ShippingMethodName { get; set; } = string.Empty;
        public decimal? ShippingFreeThreshold { get; set; }
        public bool ShippingIsPickup { get; set; }
        public string? ShippingEstimatedDelivery { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
