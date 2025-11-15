namespace RenzoAgostini.Server.Entities
{
    public class ShippingOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public decimal? FreeShippingThreshold { get; set; }
        public bool SupportsItaly { get; set; } = true;
        public bool SupportsInternational { get; set; }
        public bool IsPickup { get; set; }
        public string? EstimatedDelivery { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
