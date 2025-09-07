using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface IShippingClient
    {
        Task<IReadOnlyList<ShippingOptionDto>> GetOptionsAsync(string country);
    }
}
