using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface ICartService
    {
        IReadOnlyList<PaintingDto> Items { get; }
        int ItemsCount { get; }
        decimal TotalAmount { get; }
        CheckoutDto? CheckoutData { get; set; }

        event Action? OnChange;

        void AddItem(PaintingDto painting);
        void Clear();
        void RemoveItem(int paintingId);
    }
}
