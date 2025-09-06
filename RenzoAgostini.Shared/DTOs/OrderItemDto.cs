namespace RenzoAgostini.Shared.DTOs
{
    public record OrderItemDto(
            int PaintingId,
            string PaintingTitle,
            decimal Price,
            int Quantity
        );
}