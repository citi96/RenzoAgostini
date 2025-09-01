namespace RenzoAgostini.Server.Entities
{
    public class Painting
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Year { get; set; }
        public string? Medium { get; set; }
        public decimal? Price { get; set; }
        public bool IsForSale { get; set; }

        public List<PaintingImage> Images { get; set; } = new();

    }
}
