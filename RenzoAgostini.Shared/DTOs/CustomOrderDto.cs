using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Shared.DTOs
{
    public record CustomOrderDto(
        int Id,
        string CustomerEmail,
        string Description,
        string? AttachmentOriginalName,
        CustomOrderStatus Status,
        DateTime CreatedAt,
        DateTime? AcceptedAt,
        decimal? QuotedPrice,
        string? ArtistNotes,
        int? PaintingId,
        string AccessCode
    );
}
