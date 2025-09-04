using Microsoft.AspNetCore.Components;

namespace RenzoAgostini.Client.Components
{
    public partial class PaintingCard : ComponentBase
    {
        [Parameter] public required string Title { get; set; }
        [Parameter] public string? Description { get; set; }
        [Parameter] public int? Year { get; set; }
        [Parameter] public string? Medium { get; set; }
        [Parameter] public decimal? Price { get; set; }
        [Parameter] public bool IsForSale { get; set; }
        [Parameter] public IReadOnlyList<string> ImageUrls { get; set; } = [];

        protected string PrimaryImageUrl => ImageUrls.Count > 0 ? ImageUrls[0] : string.Empty;
    }
}
