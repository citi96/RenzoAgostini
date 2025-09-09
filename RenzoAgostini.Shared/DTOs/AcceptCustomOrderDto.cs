namespace RenzoAgostini.Shared.DTOs
{
    public record AcceptCustomOrderDto(
        decimal QuotedPrice,
        string? ArtistNotes,
        CreatePaintingDto PaintingData
    );
}
