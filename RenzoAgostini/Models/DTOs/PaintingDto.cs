namespace RenzoAgostini.Models.DTOs
{
    public record PaintingDto(
        int Id,
        string Slug,
        string Title,
        string? Description,
        int? Year,
        string? Medium,
        decimal? Price,
        bool IsForSale,
        IReadOnlyList<string> ImageUrls
    );

    public record CreatePaintingDto(
        string Slug,
        string Title,
        string? Description,
        int? Year,
        string? Medium,
        decimal? Price,
        bool IsForSale,
        IReadOnlyList<PaintingImageDto> Images
    );
}
