using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class CartService : ICartService
    {
        private readonly List<PaintingDto> _items = new();
        public IReadOnlyList<PaintingDto> Items => _items.AsReadOnly();

        public event Action? OnChange;

        public int ItemsCount => _items.Count;
        public decimal TotalAmount => _items.Sum(p => p.Price ?? 0m);

        public CheckoutDto? CheckoutData { get; set; }

        public void AddItem(PaintingDto painting)
        {
            if (_items.Any(p => p.Id == painting.Id))
                return;
            _items.Add(painting);
            OnChange?.Invoke();
        }

        public void RemoveItem(int paintingId)
        {
            var index = _items.FindIndex(p => p.Id == paintingId);
            if (index >= 0)
            {
                _items.RemoveAt(index);
                OnChange?.Invoke();
            }
        }

        public void Clear()
        {
            _items.Clear();
            OnChange?.Invoke();
        }
    }
}
