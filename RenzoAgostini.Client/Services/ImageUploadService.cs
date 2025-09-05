using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Services
{
    public class ImageUploadService(HttpClient httpClient, ILogger<ImageUploadService> logger) : IImageUploadService
    {
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(imageStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(fileName));
                content.Add(streamContent, "file", fileName);

                var response = await httpClient.PostAsync("api/images", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return result.Trim('"'); // Remove quotes from JSON string
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading image {FileName}", fileName);
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                var encodedUrl = Uri.EscapeDataString(imageUrl);
                await httpClient.DeleteAsync($"api/images?url={encodedUrl}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting image {ImageUrl}", imageUrl);
                // Non lanciare eccezione per la cancellazione
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}