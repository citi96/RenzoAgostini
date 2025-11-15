using System.Threading;
using System.Threading.Tasks;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface ICartService
    {
        IReadOnlyList<PaintingDto> Items { get; }
        int ItemsCount { get; }
        decimal TotalAmount { get; }
        CheckoutDto? CheckoutData { get; set; }
        ShippingOptionDto? SelectedShippingOption { get; }

        event Action? OnChange;

        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaintingDto>> GetItemsAsync(CancellationToken cancellationToken = default);
        Task AddItemAsync(PaintingDto painting, CancellationToken cancellationToken = default);
        Task RemoveItemAsync(int paintingId, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
