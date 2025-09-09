using RenzoAgostini.Shared.Common;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services.Interfaces
{
    public interface ICustomOrderService
    {
        Task<Result<CustomOrderDto>> CreateCustomOrderAsync(CreateCustomOrderDto dto);
        Task<Result<CustomOrderDto>> AcceptCustomOrderAsync(int customOrderId, AcceptCustomOrderDto dto);
        Task<Result<CustomOrderDto>> RejectCustomOrderAsync(int customOrderId, string? reason);
        Task<Result<CustomOrderDto>> GetByAccessCodeAsync(string accessCode, string customerEmail);
        Task<IEnumerable<CustomOrderDto>> GetAllCustomOrdersAsync();
    }
}
