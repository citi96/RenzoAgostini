using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Shared.Contracts
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> UpdateOrderTrackingAsync(int orderId, string trackingNumber);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}
