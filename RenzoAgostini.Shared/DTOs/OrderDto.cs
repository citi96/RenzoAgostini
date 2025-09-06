using RenzoAgostini.Shared.Data;

namespace RenzoAgostini.Shared.DTOs
{
    public record OrderDto(
            int Id,
            string CustomerFirstName,
            string CustomerLastName,
            string Email,
            OrderStatus Status,
            DateTime OrderDate,
            decimal Total,
            int ItemCount,
            string PaymentMethod,
            AddressDto Address,
            string? TrackingNumber,
            IReadOnlyList<OrderItemDto> Items
        );
}