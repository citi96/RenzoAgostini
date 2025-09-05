using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RenzoAgostini.Client.Services;
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

        private PaintingForm form = new();
        private bool IsEditing => Painting != null;
        private bool CanSave => !string.IsNullOrWhiteSpace(form.Title) &&
                               !string.IsNullOrWhiteSpace(form.Slug) &&
                               (IsEditing || form.Images.Any());

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

        private void SetPrimary(int index)
        {
            for (int i = 0; i < form.Images.Count; i++)
            {
                var currentImage = form.Images[i];
                form.Images[i] = currentImage with { IsPrimary = i == index };
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                var dto = form.ToCreateDto();

                if (IsEditing)
                {
                    await PaintingService.UpdatePaintingAsync(Painting!.Id, dto);
                }
                else
                {
                    await PaintingService.CreatePaintingAsync(dto);
                }

                await OnSave.InvokeAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving painting");
            }
        }

        private async Task HandleFileSelection(InputFileChangeEventArgs e)
        {
            const long maxFileSize = 5 * 1024 * 1024;

            foreach (var file in e.GetMultipleFiles())
            {
                if (file.Size > maxFileSize)
                {
                    Logger.LogWarning("File {FileName} too large: {Size} bytes", file.Name, file.Size);
                    continue;
                }

                try
                {
                    // Upload reale del file
                    using var stream = file.OpenReadStream(maxFileSize);
                    var imageUrl = await ImageUploadService.UploadImageAsync(stream, file.Name);

                    var imageDto = new PaintingImageDto(
                        Url: imageUrl,
                        Width: null,
                        Height: null,
                        IsPrimary: !form.Images.Any()
                    );

                    form.Images.Add(imageDto);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error uploading file {FileName}", file.Name);
                }
            }

            StateHasChanged();
        }

        private async Task RemoveImage(int index)
        {
            var imageToRemove = form.Images[index];
            var wasMainImage = imageToRemove.IsPrimary;

            // Elimina l'immagine dal server se non è una data URL
            if (!imageToRemove.Url.StartsWith("data:"))
            {
                await ImageUploadService.DeleteImageAsync(imageToRemove.Url);
            }

            form.Images.RemoveAt(index);

            if (wasMainImage && form.Images.Any())
            {
                form.Images[0] = form.Images[0] with { IsPrimary = true };
            }
        }
    }

    public class PaintingForm
    {
        [Required(ErrorMessage = "Il titolo è obbligatorio")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lo slug è obbligatorio")]
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