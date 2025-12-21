using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Client.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace RenzoAgostini.Client.Pages
{
    public partial class Biography : ComponentBase
    {
        [Inject] protected IConfiguration Configuration { get; set; } = default!;
        [Inject] protected IBiographyService BioService { get; set; } = default!;
        [Inject] protected IImageUploadService ImageService { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] protected NavigationManager NavManager { get; set; } = default!;

        protected bool isLoading = true;
        protected bool isEditing = false;
        protected bool isSaving = false;
        protected BiographyDto displayDto = new("", null);
        protected BiographyForm editForm = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                displayDto = await BioService.GetAsync();
            }
            catch (Exception)
            {
                // Handle error or leave empty
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void EnableEditing()
        {
            editForm = new BiographyForm
            {
                Content = displayDto.Content,
                ImageUrl = displayDto.ImageUrl
            };
            isEditing = true;
        }

        protected void CancelEditing()
        {
            isEditing = false;
            editForm = new(); // Clear
        }

        protected async Task HandleImageUpload(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file != null)
                {
                    // Max 5MB
                    var maxFileSize = 5L * 1024 * 1024;
                    using var stream = file.OpenReadStream(maxFileSize);
                    var imageUrl = await ImageService.UploadImageAsync(stream, file.Name);
                    editForm.ImageUrl = imageUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
            }
        }

        protected void RemoveImage()
        {
            editForm.ImageUrl = null;
        }

        protected async Task SaveBio()
        {
            isSaving = true;
            try
            {
                var dto = new BiographyDto(editForm.Content, editForm.ImageUrl);
                await BioService.UpdateAsync(dto);
                displayDto = dto;
                isEditing = false;
            }
            catch (Exception)
            {
                // Show error
            }
            finally
            {
                isSaving = false;
            }
        }

        protected string GetImageUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            if (url.StartsWith("http") || url.StartsWith("data:")) return url;

            var baseUrl = Configuration["BaseUrl"]?.TrimEnd('/');
            var cleanUrl = url.TrimStart('/');

            return $"{baseUrl}/{cleanUrl}";
        }
        protected class BiographyForm
        {
            public string Content { get; set; } = string.Empty;
            public string? ImageUrl { get; set; }
        }
    }
}
