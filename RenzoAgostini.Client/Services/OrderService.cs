using System.Net.Http.Json;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class OrderService(HttpClient client) : IOrderService
    {
        private const string BasePath = "api/orders";

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var result = await client.GetFromJsonAsync<IEnumerable<OrderDto>>($"{BasePath}");
            return result ?? [];
        }

        public async Task<OrderDto> UpdateOrderTrackingAsync(int orderId, string trackingNumber)
        {
            // Aggiorna il tracking number (PUT /api/orders/{id}/tracking)
            var response = await client.PutAsJsonAsync($"{BasePath}/{orderId}/tracking", new UpdateTrackingDto(trackingNumber));
            response.EnsureSuccessStatusCode();
            var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
            return updatedOrder!;
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            // Aggiorna lo stato dell'ordine (PUT /api/orders/{id}/status)
            var response = await client.PutAsJsonAsync($"{BasePath}/{orderId}/status", new UpdateStatusDto(newStatus));
            response.EnsureSuccessStatusCode();
            var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
            return updatedOrder!;
        }
    }
}
