using Microsoft.AspNetCore.Components;
using System.Timers;

namespace RenzoAgostini.Client.Components
{
    public partial class ErrorAlert : ComponentBase, IDisposable
    {
        [Parameter] public string Message { get; set; } = string.Empty;
        [Parameter] public string? Title { get; set; }
        [Parameter] public string Type { get; set; } = "info"; // info, success, warning, error, loading
        [Parameter] public bool IsVisible { get; set; } = true;
        [Parameter] public bool IsDismissible { get; set; } = true;
        [Parameter] public bool AutoDismiss { get; set; } = false;
        [Parameter] public int AutoDismissSeconds { get; set; } = 5;
        [Parameter] public bool ShowIcon { get; set; } = true;
        [Parameter] public bool IsCompact { get; set; } = false;
        [Parameter] public bool IsLarge { get; set; } = false;
        [Parameter] public bool IsBorderless { get; set; } = false;
        [Parameter] public bool IsRounded { get; set; } = false;
        [Parameter] public bool IsRich { get; set; } = false;
        [Parameter] public bool ShowMetadata { get; set; } = false;
        [Parameter] public bool ShowTimestamp { get; set; } = false;
        [Parameter] public string? Source { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public RenderFragment? Actions { get; set; }

        [Inject] private ILogger<ErrorAlert> Logger { get; set; } = default!;

        protected bool HasActions => Actions != null;
        protected string SizeClass => IsCompact ? "alert-compact" : IsLarge ? "alert-large" : "";

        private bool isDismissing = false;
        private double progressWidth = 100;
        private System.Timers.Timer? autoDismissTimer;
        private System.Timers.Timer? progressTimer;

        protected override void OnInitialized()
        {
            if (AutoDismiss && AutoDismissSeconds > 0 && IsVisible)
            {
                SetupAutoDismiss();
            }
        }

        protected override void OnParametersSet()
        {
            // Reset dismissing state when parameters change
            if (IsVisible && isDismissing)
            {
                isDismissing = false;
                progressWidth = 100;
            }

            // Setup auto dismiss if parameters changed
            if (AutoDismiss && AutoDismissSeconds > 0 && IsVisible)
            {
                SetupAutoDismiss();
            }
            else
            {
                CleanupTimers();
            }
        }

        private void SetupAutoDismiss()
        {
            CleanupTimers();

            try
            {
                // Progress timer for visual feedback
                progressTimer = new System.Timers.Timer(50); // Update every 50ms
                progressTimer.Elapsed += UpdateProgress;
                progressTimer.AutoReset = true;
                progressTimer.Start();

                // Auto dismiss timer
                autoDismissTimer = new System.Timers.Timer(AutoDismissSeconds * 1000);
                autoDismissTimer.Elapsed += OnAutoDismiss;
                autoDismissTimer.AutoReset = false;
                autoDismissTimer.Start();

                Logger.LogDebug("Auto dismiss setup for {Seconds} seconds", AutoDismissSeconds);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error setting up auto dismiss");
            }
        }

        private void UpdateProgress(object? sender, ElapsedEventArgs e)
        {
            if (autoDismissTimer == null) return;

            var elapsed = AutoDismissSeconds * 1000 - autoDismissTimer.Interval;
            var totalTime = AutoDismissSeconds * 1000;
            progressWidth = Math.Max(0, ((totalTime - elapsed) / totalTime) * 100);

            InvokeAsync(StateHasChanged);
        }

        private void OnAutoDismiss(object? sender, ElapsedEventArgs e)
        {
            InvokeAsync(async () =>
            {
                await HandleClose();
            });
        }

        protected async Task HandleClose()
        {
            try
            {
                CleanupTimers();

                if (OnClose.HasDelegate)
                {
                    isDismissing = true;
                    StateHasChanged();

                    // Wait for animation to complete
                    await Task.Delay(200);

                    await OnClose.InvokeAsync();
                    Logger.LogDebug("Alert closed");
                }
                else
                {
                    // If no close handler, just hide
                    IsVisible = false;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error closing alert");
            }
        }

        protected string GetIcon()
        {
            return Type.ToLower() switch
            {
                "success" => "✅",
                "warning" => "⚠️",
                "error" or "danger" => "❌",
                "loading" => "⏳",
                "info" or _ => "ℹ️"
            };
        }

        private void CleanupTimers()
        {
            if (autoDismissTimer != null)
            {
                autoDismissTimer.Stop();
                autoDismissTimer.Dispose();
                autoDismissTimer = null;
            }

            if (progressTimer != null)
            {
                progressTimer.Stop();
                progressTimer.Dispose();
                progressTimer = null;
            }
        }

        public void Dispose()
        {
            CleanupTimers();
        }

        // Public methods for external control
        public async Task Show()
        {
            IsVisible = true;
            isDismissing = false;
            progressWidth = 100;

            if (AutoDismiss && AutoDismissSeconds > 0)
            {
                SetupAutoDismiss();
            }

            await InvokeAsync(StateHasChanged);
        }

        public async Task Hide()
        {
            await HandleClose();
        }

        public void StopAutoDismiss()
        {
            CleanupTimers();
            progressWidth = 100;
            StateHasChanged();
        }

        public void RestartAutoDismiss()
        {
            if (AutoDismiss && AutoDismissSeconds > 0)
            {
                SetupAutoDismiss();
            }
        }
    }

    // Helper class for creating alerts programmatically
    public static class AlertHelper
    {
        public static RenderFragment CreateAlert(
            string message,
            string type = "info",
            string? title = null,
            bool autoDismiss = false,
            int autoDismissSeconds = 5,
            EventCallback? onClose = null)
        {
            return builder =>
            {
                builder.OpenComponent<ErrorAlert>(0);
                builder.AddAttribute(1, "Message", message);
                builder.AddAttribute(2, "Type", type);
                if (!string.IsNullOrEmpty(title))
                    builder.AddAttribute(3, "Title", title);
                builder.AddAttribute(4, "AutoDismiss", autoDismiss);
                builder.AddAttribute(5, "AutoDismissSeconds", autoDismissSeconds);
                if (onClose.HasValue)
                    builder.AddAttribute(6, "OnClose", onClose.Value);
                builder.CloseComponent();
            };
        }

        public static RenderFragment CreateSuccessAlert(string message, bool autoDismiss = true)
            => CreateAlert(message, "success", "Successo", autoDismiss);

        public static RenderFragment CreateErrorAlert(string message, string? title = null)
            => CreateAlert(message, "error", title ?? "Errore");

        public static RenderFragment CreateWarningAlert(string message, bool autoDismiss = true)
            => CreateAlert(message, "warning", "Attenzione", autoDismiss);

        public static RenderFragment CreateInfoAlert(string message, bool autoDismiss = true)
            => CreateAlert(message, "info", "Informazione", autoDismiss);

        public static RenderFragment CreateLoadingAlert(string message)
            => CreateAlert(message, "loading", "Caricamento", false);
    }
}