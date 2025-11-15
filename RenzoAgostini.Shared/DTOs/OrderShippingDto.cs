namespace RenzoAgostini.Shared.DTOs
{
    public record OrderShippingDto(
        int? ShippingOptionId,
        string Method,
        decimal Cost,
        decimal? FreeShippingThreshold,
        bool IsPickup,
        string? EstimatedDelivery
    );
}
