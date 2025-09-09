using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Server.Entities
{
    public class CustomOrder
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AttachmentPath { get; set; }
        public string? AttachmentOriginalName { get; set; }
        public CustomOrderStatus Status { get; set; } = CustomOrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public decimal? QuotedPrice { get; set; }
        public string? ArtistNotes { get; set; }

        public int? PaintingId { get; set; }
        public Painting? Painting { get; set; }

        public string AccessCode { get; set; } = string.Empty;
    }
}
