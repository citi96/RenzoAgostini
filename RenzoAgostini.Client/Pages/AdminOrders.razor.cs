using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class AdminOrders : ComponentBase
    {
        [Inject] private IOrderService OrderService { get; set; } = default!;
        [Inject] private ILogger<AdminOrders> Logger { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private List<OrderDto>? orders;
        private OrderDto? selectedOrder;
        private bool showDetail = false;
        private string? errorMessage;
        private string? successMessage;
        private string newTrackingNumber = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            try
            {
                orders = (await OrderService.GetAllOrdersAsync())?.ToList();

                // Ordina per data decrescente
                if (orders != null)
                {
                    orders = orders.OrderByDescending(o => o.OrderDate).ToList();
                }

                errorMessage = null;
                Logger.LogInformation("Loaded {Count} orders successfully", orders?.Count ?? 0);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading orders");
                errorMessage = "Errore nel caricamento degli ordini.";
                await ShowErrorToast("Errore nel caricamento degli ordini");
            }
        }

        private void ShowDetails(OrderDto order)
        {
            selectedOrder = order;
            newTrackingNumber = order.TrackingNumber ?? string.Empty;
            showDetail = true;
            StateHasChanged();
        }

        private void CloseDetail()
        {
            showDetail = false;
            selectedOrder = null;
            newTrackingNumber = string.Empty;
            StateHasChanged();
        }

        private async Task UpdateTracking()
        {
            if (selectedOrder is null || string.IsNullOrWhiteSpace(newTrackingNumber))
                return;

            try
            {
                await OrderService.UpdateOrderTrackingAsync(selectedOrder.Id, newTrackingNumber.Trim());

                // Aggiorna l'ordine locale
                if (orders != null)
                {
                    var orderIndex = orders.FindIndex(o => o.Id == selectedOrder.Id);
                    if (orderIndex >= 0)
                    {
                        orders[orderIndex] = orders[orderIndex] with { TrackingNumber = newTrackingNumber.Trim() };
                        selectedOrder = orders[orderIndex];
                    }
                }

                successMessage = "Tracking aggiornato con successo.";
                await ShowSuccessToast("Tracking aggiornato con successo!");

                Logger.LogInformation("Tracking updated for order {OrderId}: {TrackingNumber}",
                    selectedOrder.Id, newTrackingNumber);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating tracking for order {OrderId}", selectedOrder.Id);
                errorMessage = "Errore durante l'aggiornamento del tracking.";
                await ShowErrorToast("Errore durante l'aggiornamento del tracking");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task MarkAsShipped()
        {
            if (selectedOrder is null) return;

            try
            {
                // Aggiorna il tracking se inserito
                if (!string.IsNullOrWhiteSpace(newTrackingNumber) &&
                    newTrackingNumber.Trim() != (selectedOrder.TrackingNumber ?? string.Empty))
                {
                    await OrderService.UpdateOrderTrackingAsync(selectedOrder.Id, newTrackingNumber.Trim());
                }

                // Cambia lo stato in spedito
                await OrderService.UpdateOrderStatusAsync(selectedOrder.Id, OrderStatus.Shipped);

                // Aggiorna gli ordini locali
                if (orders != null)
                {
                    var orderIndex = orders.FindIndex(o => o.Id == selectedOrder.Id);
                    if (orderIndex >= 0)
                    {
                        orders[orderIndex] = orders[orderIndex] with
                        {
                            Status = OrderStatus.Shipped,
                            TrackingNumber = newTrackingNumber.Trim()
                        };
                        selectedOrder = orders[orderIndex];
                    }
                }

                successMessage = "Ordine segnato come spedito con successo.";
                await ShowSuccessToast("Ordine spedito con successo!");

                Logger.LogInformation("Order {OrderId} marked as shipped", selectedOrder.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error marking order {OrderId} as shipped", selectedOrder.Id);
                errorMessage = "Errore durante l'aggiornamento dello stato dell'ordine.";
                await ShowErrorToast("Errore durante l'aggiornamento dell'ordine");
            }
            finally
            {
                StateHasChanged();
            }
        }

        // Helper methods per UI
        private static string GetStatusLabel(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "In Attesa",
                OrderStatus.Paid => "Pagato",
                OrderStatus.Shipped => "Spedito",
                OrderStatus.Cancelled => "Cancellato",
                _ => status.ToString()
            };
        }

        private static string GetStatusClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "pending",
                OrderStatus.Paid => "paid",
                OrderStatus.Shipped => "shipped",
                OrderStatus.Cancelled => "cancelled",
                _ => "pending"
            };
        }

        // Toast notifications
        private async Task ShowSuccessToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "success");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing success toast");
            }
        }

        private async Task ShowErrorToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "error");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing error toast");
            }
        }
    }
}