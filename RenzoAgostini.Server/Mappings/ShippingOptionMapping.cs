using RenzoAgostini.Server.Entities;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Mappings
{
    public static class ShippingOptionMapping
    {
        public static ShippingOptionDto ToDto(this ShippingOption option)
        {
            return new ShippingOptionDto(
                option.Id,
                option.Name,
                option.Description,
                option.Cost,
                option.FreeShippingThreshold,
                option.SupportsItaly,
                option.SupportsInternational,
                option.IsPickup,
                option.EstimatedDelivery,
                option.IsActive
            );
        }
    }
}
