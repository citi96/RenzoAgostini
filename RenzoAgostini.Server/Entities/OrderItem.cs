namespace RenzoAgostini.Server.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public int PaintingId { get; set; }
        public Painting Painting { get; set; } = default!;
        public string PaintingTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
