namespace RenzoAgostini.Models
{
    public record Painting(
    int Id,
    string Slug,
    string Title,
    string? Description,
    int? Year,
    string? Medium,
    decimal? Price,
    bool IsForSale,
    IReadOnlyList<PaintingImage> Images
);
}
