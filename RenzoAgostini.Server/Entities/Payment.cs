using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Server.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int PaintingId { get; set; }
        public string? UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string? StripeSessionId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Painting? Painting { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
