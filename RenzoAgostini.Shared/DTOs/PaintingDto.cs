namespace RenzoAgostini.Shared.DTOs
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
        string? Dimensions,
        IReadOnlyList<string> ImageUrls
    );
}
