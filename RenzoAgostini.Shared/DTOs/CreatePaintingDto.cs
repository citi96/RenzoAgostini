namespace RenzoAgostini.Shared.DTOs
{
    public record CreatePaintingDto(
        string Slug,
        string Title,
        string? Description,
        int? Year,
        string? Medium,
        decimal? Price,
        bool IsForSale,
        string? Dimensions,
        IReadOnlyList<PaintingImageDto> Images
    )
    {
        public string Slug {  get; set; } = Slug;
        public string Title { get; set; } = Title;
        public string? Description { get; set; } = Description;
        public int? Year { get; set; } = Year;
        public string? Medium { get; set; } = Medium;
        public decimal? Price { get; set; } = Price;
        public bool IsForSale { get; set; } = IsForSale;
        public string? Dimensions { get; set; } = Dimensions;
        public IReadOnlyList<PaintingImageDto> Images { get; set; } = Images;
    };
}
