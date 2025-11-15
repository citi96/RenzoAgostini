using RenzoAgostini.Server.Entities;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Mappings
{
    public static class OrderMapping
    {
        public static OrderDto ToOrderAdminDto(this Order order)
        {
            var address = new AddressDto(
                order.AddressLine,
                order.City,
                order.PostalCode,
                order.Country
            );

            var itemsDto = order.Items.Select(item => new OrderItemDto(
                item.PaintingId,
                item.PaintingTitle,
                item.Price,
                1
            )).ToList();

            string paymentMethod = "Stripe";

            var shipping = new OrderShippingDto(
                order.ShippingOptionId,
                order.ShippingMethodName,
                order.ShippingCost,
                order.ShippingFreeThreshold,
                order.ShippingIsPickup,
                order.ShippingEstimatedDelivery
            );

            return new OrderDto(
                order.Id,
                order.CustomerFirstName,
                order.CustomerLastName,
                order.CustomerEmail,
                order.Status,
                order.CreatedAt,
                order.ItemsTotal,
                order.ShippingCost,
                order.TotalAmount,
                itemsDto.Count,
                paymentMethod,
                address,
                order.TrackingNumber,
                shipping,
                itemsDto
            );
        }
    }
}
