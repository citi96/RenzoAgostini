using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services.Interfaces
{
    public interface IShippingOptionService
    {
        Task<IReadOnlyList<ShippingOptionDto>> GetActiveForCountryAsync(string country);
        Task<IReadOnlyList<ShippingOptionDto>> GetAllAsync();
        Task<ShippingOptionDto> CreateAsync(CreateShippingOptionDto dto);
        Task<ShippingOptionDto> UpdateAsync(int id, UpdateShippingOptionDto dto);
        Task DeleteAsync(int id);
    }
}
