namespace RenzoAgostini.Shared.DTOs
{
    public record CreateShippingOptionDto(
        string Name,
        string? Description,
        decimal Cost,
        decimal? FreeShippingThreshold,
        bool SupportsItaly,
        bool SupportsInternational,
        bool IsPickup,
        string? EstimatedDelivery,
        bool IsActive
    );
}
