using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Components
{
    public partial class PaintingEditor : ComponentBase
    {
        [Parameter] public PaintingDto? Painting { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private ILogger<PaintingEditor> Logger { get; set; } = default!;
        [Inject] private IImageUploadService ImageUploadService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private PaintingForm form = new();
        private bool isSaving = false;

        protected bool IsEditing => Painting != null;
        protected bool CanSave => !string.IsNullOrWhiteSpace(form.Title) &&
                                 (IsEditing || form.Images.Any()) &&
                                 !isSaving;

        protected override void OnParametersSet()
        {
            if (Painting != null)
            {
                form = PaintingForm.FromDto(Painting);
            }
            else
            {
                form = new PaintingForm();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    // Focus sul primo input
                    await JSRuntime.InvokeVoidAsync("focusElement", ".editor-input");

                    // Trap focus nel modal per accessibilità
                    await JSRuntime.InvokeVoidAsync("trapFocus", ".editor-modal");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error setting up editor modal");
                }
            }
        }

        private void SetPrimary(int index)
        {
            for (int i = 0; i < form.Images.Count; i++)
            {
                var currentImage = form.Images[i];
                form.Images[i] = currentImage with { IsPrimary = i == index };
            }
            StateHasChanged();
        }

        private async Task HandleSubmit()
        {
            if (isSaving) return;

            try
            {
                isSaving = true;
                StateHasChanged();

                if (string.IsNullOrWhiteSpace(form.Slug))
                {
                    form.Slug = GenerateSlug(form.Title);
                }

                var dto = form.ToCreateDto();

                if (IsEditing)
                {
                    await PaintingService.UpdatePaintingAsync(Painting!.Id, dto);
                    await ShowSuccessToast("Quadro modificato con successo!");
                    Logger.LogInformation("Painting {PaintingId} updated successfully", Painting.Id);
                }
                else
                {
                    await PaintingService.CreatePaintingAsync(dto);
                    await ShowSuccessToast("Quadro creato con successo!");
                    Logger.LogInformation("New painting created successfully");
                }

                await OnSave.InvokeAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving painting");
                await ShowErrorToast("Errore durante il salvataggio del quadro");
            }
            finally
            {
                isSaving = false;
                StateHasChanged();
            }
        }

        private async Task HandleFileSelection(InputFileChangeEventArgs e)
        {
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            const int maxFiles = 10;

            var files = e.GetMultipleFiles(maxFiles);
            var uploadedCount = 0;

            foreach (var file in files)
            {
                if (file.Size > maxFileSize)
                {
                    Logger.LogWarning("File {FileName} too large: {Size} bytes", file.Name, file.Size);
                    await ShowErrorToast($"File {file.Name} troppo grande (max 5MB)");
                    continue;
                }

                if (!IsImageFile(file.ContentType))
                {
                    Logger.LogWarning("File {FileName} is not a valid image: {ContentType}", file.Name, file.ContentType);
                    await ShowErrorToast($"File {file.Name} non è un'immagine valida");
                    continue;
                }

                try
                {
                    // Mostra preview temporaneo durante l'upload
                    var tempUrl = await CreateImagePreview(file);
                    var tempImage = new PaintingImageDto(
                        Url: tempUrl,
                        Width: null,
                        Height: null,
                        IsPrimary: !form.Images.Any()
                    );
                    form.Images.Add(tempImage);
                    StateHasChanged();

                    // Upload reale del file
                    using var stream = file.OpenReadStream(maxFileSize);
                    var imageUrl = await ImageUploadService.UploadImageAsync(stream, file.Name);

                    // Sostituisci l'URL temporaneo con quello reale
                    var imageIndex = form.Images.FindIndex(img => img.Url == tempUrl);
                    if (imageIndex >= 0)
                    {
                        form.Images[imageIndex] = tempImage with { Url = imageUrl.TrimStart('/') };
                    }

                    uploadedCount++;
                    Logger.LogInformation("Image {FileName} uploaded successfully", file.Name);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error uploading file {FileName}", file.Name);

                    // Rimuovi l'immagine temporanea in caso di errore
                    var tempImage = form.Images.FirstOrDefault(img => img.Url.StartsWith("data:"));
                    if (tempImage != null)
                    {
                        form.Images.Remove(tempImage);
                    }

                    await ShowErrorToast($"Errore caricamento {file.Name}");
                }
            }

            if (uploadedCount > 0)
            {
                await ShowSuccessToast($"{uploadedCount} immagine/i caricate con successo!");
            }

            StateHasChanged();
        }

        private async Task RemoveImage(int index)
        {
            if (index < 0 || index >= form.Images.Count) return;

            var imageToRemove = form.Images[index];
            var wasMainImage = imageToRemove.IsPrimary;

            try
            {
                // Elimina l'immagine dal server se non è una data URL
                if (!imageToRemove.Url.StartsWith("data:") && !string.IsNullOrEmpty(imageToRemove.Url))
                {
                    await ImageUploadService.DeleteImageAsync(imageToRemove.Url);
                }

                form.Images.RemoveAt(index);

                // Se era l'immagine principale e ce ne sono altre, rendi principale la prima
                if (wasMainImage && form.Images.Any())
                {
                    form.Images[0] = form.Images[0] with { IsPrimary = true };
                }

                await ShowSuccessToast("Immagine rimossa con successo");
                StateHasChanged();

                Logger.LogInformation("Image removed from painting editor at index {Index}", index);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error removing image at index {Index}", index);
                await ShowErrorToast("Errore durante la rimozione dell'immagine");
            }
        }

        // Helper methods
        private static bool IsImageFile(string contentType)
        {
            return contentType.StartsWith("image/") &&
                   (contentType.Contains("jpeg") || contentType.Contains("jpg") ||
                    contentType.Contains("png") || contentType.Contains("gif") ||
                    contentType.Contains("webp"));
        }

        private async Task<string> CreateImagePreview(IBrowserFile file)
        {
            try
            {
                const int maxPreviewSize = 100 * 1024; // 100KB per preview
                using var stream = file.OpenReadStream(maxPreviewSize);
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer);
                var base64 = Convert.ToBase64String(buffer);
                return $"data:{file.ContentType};base64,{base64}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating image preview for {FileName}", file.Name);
                return "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cGF0aCBkPSJNMTIgMkM2LjQ4IDIgMiA2LjQ4IDIgMTJzNC40OCAxMCAxMCAxMCAxMC00LjQ4IDEwLTEwUzE3LjUyIDIgMTIgMnptLTIgMTVsLTUtNSAxLjQxLTEuNDFMMTAgMTQuMTdsNy41OS03LjU5TDE5IDhsLTkgOXoiIGZpbGw9IiM5Q0EzQUYiLz48L3N2Zz4=";
            }
        }

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
        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            var slug = title.ToLowerInvariant();
            // Remove diacritics/accents could be added here if needed, keeping it simple for now
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');

            return slug;
        }
    }

    public class PaintingForm
    {
        [Required(ErrorMessage = "Il titolo è obbligatorio")]
        public string Title { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Lo slug è obbligatorio")]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int? Year { get; set; }
        public string? Medium { get; set; }
        public string? Dimensions { get; set; }
        public decimal? Price { get; set; }
        public bool IsForSale { get; set; }
        public List<PaintingImageDto> Images { get; set; } = new();

        public static PaintingForm FromDto(PaintingDto dto) => new()
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Description = dto.Description,
            Year = dto.Year,
            Medium = dto.Medium,
            Dimensions = dto.Dimensions,
            Price = dto.Price,
            IsForSale = dto.IsForSale,
            Images = dto.ImageUrls.Select((url, i) => new PaintingImageDto(
                url, null, null, i == 0)).ToList()
        };

        public CreatePaintingDto ToCreateDto() => new(
            Slug,
            Title,
            Description,
            Year,
            Medium,
            Price,
            IsForSale,
            Dimensions,
            Images.AsReadOnly()
        );
    }
}