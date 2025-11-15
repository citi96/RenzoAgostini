using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Services
{
    public class CartService : ICartService
    {
        private const string StorageKey = "renzo-agostini-cart";

        private readonly List<PaintingDto> _items = new();
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<CartService> _logger;
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private bool _isInitialized;
        private ShippingOptionDto? _selectedShippingOption;

        public CartService(IJSRuntime jsRuntime, ILogger<CartService> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public IReadOnlyList<PaintingDto> Items => _items.AsReadOnly();
        public int ItemsCount => _items.Count;
        public decimal TotalAmount => _items.Sum(p => p.Price ?? 0m);

        public CheckoutDto? CheckoutData { get; set; }
        public ShippingOptionDto? SelectedShippingOption => _selectedShippingOption;

        public event Action? OnChange;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized)
            {
                return;
            }

            await _initializationLock.WaitAsync(cancellationToken);
            try
            {
                if (_isInitialized)
                {
                    return;
                }

                try
                {
                    var storedItems = await _jsRuntime.InvokeAsync<string?>(
                        "localStorage.getItem",
                        cancellationToken,
                        StorageKey);

                    if (!string.IsNullOrWhiteSpace(storedItems))
                    {
                        var deserialized = JsonSerializer.Deserialize<List<PaintingDto>>(storedItems, _serializerOptions);
                        if (deserialized is { Count: > 0 })
                        {
                            _items.Clear();
                            _items.AddRange(deserialized);
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Unable to deserialize cart content");
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize cart from localStorage");
                }
                finally
                {
                    _isInitialized = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        public async Task<IReadOnlyList<PaintingDto>> GetItemsAsync(CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            return Items;
        }

        public async Task AddItemAsync(PaintingDto painting, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(painting);

            await InitializeAsync(cancellationToken);

            if (_items.Any(p => p.Id == painting.Id))
            {
                return;
            }

            _items.Add(painting);
            await PersistAsync(cancellationToken);
            OnChange?.Invoke();
        }

        public async Task RemoveItemAsync(int paintingId, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);

            var index = _items.FindIndex(p => p.Id == paintingId);
            if (index < 0)
            {
                return;
            }

            _items.RemoveAt(index);
            if (_items.Count == 0)
            {
                _selectedShippingOption = null;
            }
            await PersistAsync(cancellationToken);
            OnChange?.Invoke();
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);

            if (_items.Count == 0)
            {
                if (_selectedShippingOption is not null)
                {
                    _selectedShippingOption = null;
                }
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
                OnChange?.Invoke();
                return;
            }

            _items.Clear();
            _selectedShippingOption = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
            OnChange?.Invoke();
        }

        public void SetShippingOption(ShippingOptionDto? option)
        {
            if (Equals(_selectedShippingOption, option))
            {
                return;
            }

            _selectedShippingOption = option;
            OnChange?.Invoke();
        }

        private async Task PersistAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_items.Count == 0)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
                    return;
                }

                var payload = JsonSerializer.Serialize(_items, _serializerOptions);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist cart state to localStorage");
            }
        }
    }
}
