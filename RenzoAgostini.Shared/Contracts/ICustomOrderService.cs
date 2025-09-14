using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Shared.Contracts
{
    public interface ICustomOrderService
    {
        Task<CustomOrderDto> CreateCustomOrderAsync(CreateCustomOrderDto dto);
        Task<CustomOrderDto> GetByAccessCodeAsync(string accessCode, string customerEmail);
        Task<IEnumerable<CustomOrderDto>> GetAllCustomOrdersAsync();
        Task<CustomOrderDto> AcceptCustomOrderAsync(int customOrderId, AcceptCustomOrderDto dto);
        Task<CustomOrderDto> RejectCustomOrderAsync(int customOrderId, string? reason);
    }
}
