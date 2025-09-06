using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class AdminOrders : ComponentBase
    {
        [Inject] private IOrderService OrderService { get; set; } = default!;
        [Inject] private ILogger<AdminOrders> Logger { get; set; } = default!;

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
                errorMessage = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading orders");
                errorMessage = "Errore nel caricamento degli ordini.";
            }
        }

        private void ShowDetails(OrderDto order)
        {
            // Mostra il pannello dei dettagli per l'ordine selezionato
            selectedOrder = order;
            newTrackingNumber = order.TrackingNumber ?? string.Empty;
            showDetail = true;
        }

        private void CloseDetail()
        {
            // Chiude il pannello dei dettagli senza salvare modifiche
            showDetail = false;
            selectedOrder = null;
            newTrackingNumber = string.Empty;
        }

        private async Task UpdateTracking()
        {
            if (selectedOrder is null) return;
            try
            {
                // Aggiorna il tracking number tramite API
                await OrderService.UpdateOrderTrackingAsync(selectedOrder.Id, newTrackingNumber);
                // Ricarica la lista ordini per visualizzare il cambiamento
                await LoadOrders();
                successMessage = "Tracking aggiornato con successo.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating tracking for order {OrderId}", selectedOrder.Id);
                errorMessage = "Errore durante l'aggiornamento del tracking.";
            }
            finally
            {
                CloseDetail();
            }
        }

        private async Task MarkAsShipped()
        {
            if (selectedOrder is null) return;
            // Aggiorna il tracking (se inserito) prima di cambiare lo stato
            if (!string.IsNullOrWhiteSpace(newTrackingNumber))
            {
                try
                {
                    await OrderService.UpdateOrderTrackingAsync(selectedOrder.Id, newTrackingNumber);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error updating tracking for order {OrderId} before shipping", selectedOrder.Id);
                    errorMessage = "Errore durante l'aggiornamento del tracking.";
                    CloseDetail();
                    return;
                }
            }
            try
            {
                // Imposta lo stato dell'ordine su "Spedito"
                await OrderService.UpdateOrderStatusAsync(selectedOrder.Id, OrderStatus.Shipped);
                successMessage = "Ordine spedito con successo.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error marking order {OrderId} as shipped", selectedOrder.Id);
                errorMessage = "Errore durante l'aggiornamento dello stato dell'ordine.";
            }
            finally
            {
                await LoadOrders();
                CloseDetail();
            }
        }

        private string GetStatusLabel(OrderStatus status)
        {
            // Restituisce l'etichetta in italiano per lo stato dell'ordine
            return status switch
            {
                OrderStatus.Pending => "In attesa",
                OrderStatus.Paid => "Pagato",
                OrderStatus.Shipped => "Spedito",
                OrderStatus.Cancelled => "Cancellato",
                _ => status.ToString()
            };
        }
    }
}
