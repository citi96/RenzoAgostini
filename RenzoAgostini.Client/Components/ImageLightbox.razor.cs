using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;

namespace RenzoAgostini.Client.Components
{
    public partial class ImageLightbox : ComponentBase, IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private ILogger<ImageLightbox> Logger { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;

        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public IReadOnlyList<string> Images { get; set; } = new List<string>();
        [Parameter] public int InitialIndex { get; set; } = 0;
        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public Dictionary<int, ImageMetaData>? ImageMetadata { get; set; }
        [Parameter] public bool ShowThumbnails { get; set; } = true;
        [Parameter] public bool EnableLoop { get; set; } = true;
        [Parameter] public int SlideshowInterval { get; set; } = 3000; // 3 seconds

        protected int CurrentIndex { get; set; }
        protected bool showThumbnails = true;
        protected bool isLooping = true;
        protected bool isImageLoading = true;
        protected bool isZoomed = false;
        protected bool isFullscreen = false;
        protected bool showInfo = false;
        protected bool isSlideshowActive = false;

        private System.Timers.Timer? slideshowTimer;
        private DotNetObjectReference<ImageLightbox>? dotNetRef;

        protected override void OnInitialized()
        {
            dotNetRef = DotNetObjectReference.Create(this);
            CurrentIndex = Math.Max(0, Math.Min(InitialIndex, Images.Count - 1));
            showThumbnails = ShowThumbnails;
            isLooping = EnableLoop;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && IsVisible)
            {
                try
                {
                    // Set up keyboard event listeners
                    await JSRuntime.InvokeVoidAsync("addEventListener", "keydown", dotNetRef, "HandleGlobalKeyDown");

                    // Focus the lightbox for keyboard navigation
                    await JSRuntime.InvokeVoidAsync("focusElement", ".lightbox-overlay");

                    // Prevent body scrolling
                    await JSRuntime.InvokeVoidAsync("eval", "document.body.style.overflow = 'hidden'");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error setting up lightbox");
                }
            }
        }

        public void Dispose()
        {
            StopSlideshow();
            dotNetRef?.Dispose();

            // Restore body scrolling
            try
            {
                JSRuntime.InvokeVoidAsync("eval", "document.body.style.overflow = ''").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error cleaning up lightbox");
            }
        }

        protected override void OnParametersSet()
        {
            if (IsVisible && Images.Any())
            {
                CurrentIndex = Math.Max(0, Math.Min(InitialIndex, Images.Count - 1));
                isImageLoading = true;
                isZoomed = false;
                StopSlideshow();
            }
        }

        protected async Task Close()
        {
            try
            {
                StopSlideshow();
                isZoomed = false;
                isFullscreen = false;
                showInfo = false;

                // Restore body scrolling
                await JSRuntime.InvokeVoidAsync("eval", "document.body.style.overflow = ''");

                await OnClose.InvokeAsync();

                Logger.LogInformation("Lightbox closed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error closing lightbox");
            }
        }

        protected void NextImage()
        {
            if (!Images.Any()) return;

            if (CurrentIndex < Images.Count - 1)
            {
                CurrentIndex++;
            }
            else if (isLooping)
            {
                CurrentIndex = 0;
            }

            isImageLoading = true;
            isZoomed = false;
            StateHasChanged();
        }

        protected void PreviousImage()
        {
            if (!Images.Any()) return;

            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
            else if (isLooping)
            {
                CurrentIndex = Images.Count - 1;
            }

            isImageLoading = true;
            isZoomed = false;
            StateHasChanged();
        }

        protected void GoToImage(int index)
        {
            if (index < 0 || index >= Images.Count || index == CurrentIndex) return;

            CurrentIndex = index;
            isImageLoading = true;
            isZoomed = false;
            StateHasChanged();
        }

        protected void ToggleZoom()
        {
            isZoomed = !isZoomed;
            StateHasChanged();
        }

        protected async Task ToggleFullscreen()
        {
            try
            {
                isFullscreen = !isFullscreen;

                if (isFullscreen)
                {
                    await JSRuntime.InvokeVoidAsync("eval", "document.documentElement.requestFullscreen?.()");
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("eval", "document.exitFullscreen?.()");
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error toggling fullscreen");
            }
        }

        protected void ToggleInfo()
        {
            showInfo = !showInfo;
            StateHasChanged();
        }

        protected void StartSlideshow()
        {
            if (Images.Count <= 1 || isSlideshowActive) return;

            try
            {
                isSlideshowActive = true;
                slideshowTimer = new System.Timers.Timer(SlideshowInterval);
                slideshowTimer.Elapsed += OnSlideshowTick;
                slideshowTimer.AutoReset = true;
                slideshowTimer.Start();

                StateHasChanged();
                Logger.LogInformation("Slideshow started");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error starting slideshow");
            }
        }

        protected void StopSlideshow()
        {
            if (slideshowTimer != null)
            {
                slideshowTimer.Stop();
                slideshowTimer.Dispose();
                slideshowTimer = null;
            }

            isSlideshowActive = false;
            StateHasChanged();
        }

        private void OnSlideshowTick(object? sender, ElapsedEventArgs e)
        {
            InvokeAsync(() =>
            {
                NextImage();
            });
        }

        protected void HandleOverlayClick()
        {
            if (!isZoomed)
            {
                InvokeAsync(Close);
            }
        }

        protected async Task HandleKeyDown(KeyboardEventArgs e)
        {
            await HandleKeyPress(e.Key);
        }

        [JSInvokable]
        public async Task HandleGlobalKeyDown(string key)
        {
            if (IsVisible)
            {
                await HandleKeyPress(key);
            }
        }

        private async Task HandleKeyPress(string key)
        {
            try
            {
                switch (key.ToLower())
                {
                    case "escape":
                        await Close();
                        break;
                    case "arrowright":
                    case "arrowdown":
                        NextImage();
                        break;
                    case "arrowleft":
                    case "arrowup":
                        PreviousImage();
                        break;
                    case "z":
                        ToggleZoom();
                        break;
                    case "f":
                        await ToggleFullscreen();
                        break;
                    case "i":
                        ToggleInfo();
                        break;
                    case "s":
                        if (isSlideshowActive)
                            StopSlideshow();
                        else
                            StartSlideshow();
                        break;
                    case "home":
                        GoToImage(0);
                        break;
                    case "end":
                        GoToImage(Images.Count - 1);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling key press: {Key}", key);
            }
        }

        protected void OnImageLoaded()
        {
            isImageLoading = false;
            StateHasChanged();
        }

        protected void OnImageError()
        {
            isImageLoading = false;
            Logger.LogWarning("Failed to load image at index {Index}", CurrentIndex);
            StateHasChanged();
        }

        // Helper methods
        protected string GetCurrentImageUrl()
        {
            if (!Images.Any() || CurrentIndex >= Images.Count) return string.Empty;

            var baseUrl = Configuration["BaseUrl"] ?? "";
            var imageUrl = Images[CurrentIndex];

            return imageUrl.StartsWith("http") ? imageUrl : $"{baseUrl}/{imageUrl}";
        }

        protected string GetThumbnailUrl(int index)
        {
            if (index < 0 || index >= Images.Count) return string.Empty;

            var baseUrl = Configuration["BaseUrl"] ?? "";
            var imageUrl = Images[index];

            // In a real app, you might want to generate thumbnail URLs
            // For now, we'll use the full image
            return imageUrl.StartsWith("http") ? imageUrl : $"{baseUrl}/{imageUrl}";
        }
    }

    // Helper class for image metadata
    public class ImageMetaData
    {
        public string? Year { get; set; }
        public string? Medium { get; set; }
        public string? Dimensions { get; set; }
        public string? Description { get; set; }
        public string? Artist { get; set; }
    }
}