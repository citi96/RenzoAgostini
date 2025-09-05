using Microsoft.AspNetCore.Components;

namespace RenzoAgostini.Client.Components
{
    public partial class ErrorAlert : ComponentBase
    {
        [Parameter] public string? Message { get; set; }
        [Parameter] public string Type { get; set; } = "error"; // error, success, warning, info
        [Parameter] public EventCallback OnClose { get; set; }

        private void Close() => OnClose.InvokeAsync();

        private string GetIcon() => Type switch
        {
            "success" => "✅",
            "warning" => "⚠️",
            "info" => "ℹ️",
            _ => "❌"
        };
    }
}