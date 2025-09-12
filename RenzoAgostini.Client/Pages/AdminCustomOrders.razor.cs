using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class AdminCustomOrders : ComponentBase
    {
        [Inject] private ICustomOrderService CustomOrderService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ILogger<AdminCustomOrders> Logger { get; set; } = default!;

        protected List<CustomOrderDto>? customOrders;
        protected List<CustomOrderDto> filteredOrders = [];
        protected CustomOrderDto? selectedOrder;
        protected string? errorMessage;
        protected string? successMessage;

        // Modals
        protected bool showDetailsModal = false;
        protected bool showAcceptModal = false;
        protected bool showRejectModal = false;

        // Forms
        protected AcceptOrderForm acceptForm = new();
        protected string rejectReason = string.Empty;

        // Loading states
        protected bool isProcessingAccept = false;
        protected bool isProcessingReject = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadCustomOrders();
        }

        protected async Task LoadCustomOrders()
        {
            try
            {
                var orders = await CustomOrderService.GetAllCustomOrdersAsync();
                customOrders = orders.ToList();
                FilterOrders();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading custom orders");
                errorMessage = ex.Message;
                StateHasChanged();
            }
        }

        protected void FilterOrders(ChangeEventArgs? e = null)
        {
            var selectedStatus = e?.Value?.ToString();

            if (customOrders == null) return;

            filteredOrders = string.IsNullOrEmpty(selectedStatus)
                ? [.. customOrders]
                : [.. customOrders.Where(o => o.Status.ToString() == selectedStatus)];

            StateHasChanged();
        }

        protected void ShowDetails(CustomOrderDto order)
        {
            selectedOrder = order;
            showDetailsModal = true;
            StateHasChanged();
        }

        protected void ShowAcceptModal(CustomOrderDto order)
        {
            selectedOrder = order;
            acceptForm = new AcceptOrderForm
            {
                QuotedPrice = 0,
                ArtistNotes = "",
                PaintingData = new(
                    $"custom-{order.Id}-{DateTime.Now:yyyyMMdd}",
                    $"Quadro personalizzato per {order.CustomerEmail}",
                    order.Description,
                    DateTime.Now.Year,
                    "Olio su tela",
                    0,
                    true,
                    "",
                    []
                )
            };
            showAcceptModal = true;
            StateHasChanged();
        }

        protected void ShowRejectModal(CustomOrderDto order)
        {
            selectedOrder = order;
            rejectReason = string.Empty;
            showRejectModal = true;
            StateHasChanged();
        }

        protected async Task AcceptOrder()
        {
            if (selectedOrder == null) return;

            try
            {
                isProcessingAccept = true;
                StateHasChanged();

                acceptForm.PaintingData.Price = acceptForm.QuotedPrice;

                var dto = new AcceptCustomOrderDto(
                    acceptForm.QuotedPrice,
                    acceptForm.ArtistNotes,
                    acceptForm.PaintingData
                );

                await CustomOrderService.AcceptCustomOrderAsync(selectedOrder.Id, dto);

                successMessage = $"Richiesta #{selectedOrder.Id} accettata con successo!";
                await LoadCustomOrders();
                CloseAcceptModal();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accepting custom order {OrderId}", selectedOrder.Id);
                errorMessage = ex.Message;
            }
            finally
            {
                isProcessingAccept = false;
                StateHasChanged();
            }
        }

        protected async Task RejectOrder()
        {
            if (selectedOrder == null) return;

            try
            {
                isProcessingReject = true;
                StateHasChanged();

                await CustomOrderService.RejectCustomOrderAsync(selectedOrder.Id, rejectReason);

                successMessage = $"Richiesta #{selectedOrder.Id} rifiutata.";
                await LoadCustomOrders();
                CloseRejectModal();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error rejecting custom order {OrderId}", selectedOrder.Id);
                errorMessage = ex.Message;
            }
            finally
            {
                isProcessingReject = false;
                StateHasChanged();
            }
        }

        protected void CloseDetailsModal()
        {
            showDetailsModal = false;
            StateHasChanged();
        }

        protected void CloseAcceptModal()
        {
            showAcceptModal = false;
            StateHasChanged();
        }

        protected void CloseRejectModal()
        {
            showRejectModal = false;
            StateHasChanged();
        }

        protected async Task DownloadAttachment(int orderId)
        {
            await JSRuntime.InvokeVoidAsync("open", $"/api/customorders/{orderId}/attachment", "_blank");
        }

        protected async Task ViewPainting(int paintingId)
        {
            await JSRuntime.InvokeVoidAsync("open", $"/quadro-personalizzato/{paintingId}", "_blank");
        }

        protected static string GetStatusLabel(CustomOrderStatus status) => status switch
        {
            CustomOrderStatus.Pending => "In Attesa",
            CustomOrderStatus.Accepted => "Accettato",
            CustomOrderStatus.Completed => "Completato",
            CustomOrderStatus.Rejected => "Rifiutato",
            _ => status.ToString()
        };

        protected static string GetStatusClass(CustomOrderStatus status) => status switch
        {
            CustomOrderStatus.Pending => "pending",
            CustomOrderStatus.Accepted => "accepted",
            CustomOrderStatus.Completed => "completed",
            CustomOrderStatus.Rejected => "rejected",
            _ => ""
        };

        protected static string GetRowClass(CustomOrderStatus status) => status switch
        {
            CustomOrderStatus.Pending => "row-pending",
            CustomOrderStatus.Rejected => "row-rejected",
            _ => ""
        };

        public class AcceptOrderForm
        {
            [Required(ErrorMessage = "Prezzo obbligatorio")]
            [Range(1, 50000, ErrorMessage = "Prezzo deve essere tra 1 e 50.000")]
            public decimal QuotedPrice { get; set; }

            public string? ArtistNotes { get; set; }

            [Required]
            public CreatePaintingDto PaintingData { get; set; } = new(
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                string.Empty,
                null,
                true,
                string.Empty,
                []
            );
        }
    }
}