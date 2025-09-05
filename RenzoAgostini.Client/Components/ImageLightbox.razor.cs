using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace RenzoAgostini.Client.Components
{
    public partial class ImageLightbox : ComponentBase, IAsyncDisposable
    {
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public IReadOnlyList<string> Images { get; set; } = [];
        [Parameter] public int InitialIndex { get; set; } = 0;
        [Parameter] public string? Title { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        private int _currentIndex = 0;
        private bool _isLoading = true;

        protected int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value >= 0 && value < Images.Count)
                {
                    _currentIndex = value;
                    _isLoading = true;
                }
            }
        }

        protected string CurrentImage => Images.Count > 0 ? Images[CurrentIndex] : string.Empty;
        protected bool IsLoading => _isLoading;

        protected override void OnParametersSet()
        {
            if (IsVisible && InitialIndex >= 0 && InitialIndex < Images.Count)
            {
                CurrentIndex = InitialIndex;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsVisible)
            {
                await JSRuntime.InvokeVoidAsync("document.body.style.setProperty", "overflow", "hidden");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("document.body.style.removeProperty", "overflow");
            }
        }

        private async Task Close()
        {
            await JSRuntime.InvokeVoidAsync("document.body.style.removeProperty", "overflow");
            await OnClose.InvokeAsync();
        }

        private void NextImage()
        {
            if (Images.Count > 1)
            {
                CurrentIndex = (CurrentIndex + 1) % Images.Count;
            }
        }

        private void PreviousImage()
        {
            if (Images.Count > 1)
            {
                CurrentIndex = CurrentIndex == 0 ? Images.Count - 1 : CurrentIndex - 1;
            }
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "Escape":
                    await Close();
                    break;
                case "ArrowRight":
                    NextImage();
                    break;
                case "ArrowLeft":
                    PreviousImage();
                    break;
            }
        }

        private void OnImageLoad()
        {
            _isLoading = false;
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("document.body.style.removeProperty", "overflow");
            }
            catch
            {
                // Ignora errori durante il dispose
            }
        }
    }
}