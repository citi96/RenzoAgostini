using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.Data;
using RenzoAgostini.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

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
        protected bool showAttachmentLightbox = false;

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
                customOrders = orders.OrderByDescending(o => o.CreatedAt).ToList();
                FilterOrders();
                StateHasChanged();
                Logger.LogInformation("Loaded {Count} custom orders successfully", customOrders.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading custom orders");
                errorMessage = ex.Message;
                await ShowErrorToast("Errore nel caricamento delle richieste");
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
                    $"Quadro personalizzato per {order.CustomerEmail.Split('@')[0]}",
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
                await ShowSuccessToast("Richiesta accettata e quadro creato!");
                await LoadCustomOrders();
                CloseAcceptModal();

                Logger.LogInformation("Custom order {OrderId} accepted successfully", selectedOrder.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accepting custom order {OrderId}", selectedOrder?.Id);
                errorMessage = ex.Message;
                await ShowErrorToast("Errore durante l'accettazione della richiesta");
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
                await ShowSuccessToast("Richiesta rifiutata");
                await LoadCustomOrders();
                CloseRejectModal();

                Logger.LogInformation("Custom order {OrderId} rejected", selectedOrder.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error rejecting custom order {OrderId}", selectedOrder?.Id);
                errorMessage = ex.Message;
                await ShowErrorToast("Errore durante il rifiuto della richiesta");
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

        protected void OpenAttachmentLightbox()
        {
            showAttachmentLightbox = true;
            StateHasChanged();
        }

        protected void CloseAttachmentLightbox()
        {
            showAttachmentLightbox = false;
            StateHasChanged();
        }

        protected async Task DownloadAttachment(int orderId)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("open", $"/api/customorders/{orderId}/attachment", "_blank");
                Logger.LogInformation("Attachment download initiated for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error downloading attachment for order {OrderId}", orderId);
                await ShowErrorToast("Errore nel download dell'allegato");
            }
        }

        protected async Task ViewPainting(int paintingId)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("open", $"/quadro-personalizzato/{paintingId}", "_blank");
                Logger.LogInformation("Painting view opened for painting {PaintingId}", paintingId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error opening painting view for painting {PaintingId}", paintingId);
                await ShowErrorToast("Errore nell'apertura del quadro");
            }
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
            _ => "pending"
        };

        protected static string GetRowClass(CustomOrderStatus status) => status switch
        {
            CustomOrderStatus.Pending => "row-pending",
            CustomOrderStatus.Rejected => "row-rejected",
            _ => ""
        };

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
        private static bool IsImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp" || ext == ".bmp";
        }

        protected void UpdateTitleAndSlug(string title)
        {
            acceptForm.PaintingData.Title = title;
            acceptForm.PaintingData.Slug = GenerateSlug(title);
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "";

            var slug = title.ToLowerInvariant().Trim();
            // Replace spaces with dashes
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            // Remove invalid chars
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            // Remove multiple dashes
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            return slug;
        }
    }
}