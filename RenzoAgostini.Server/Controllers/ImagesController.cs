using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class ImagesController(IWebHostEnvironment environment, ILogger<ImagesController> logger) : ControllerBase
    {
        private readonly string _uploadsPath = Path.Combine(environment.WebRootPath, "uploads");

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nessun file selezionato");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Tipo di file non supportato");

            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
                return BadRequest("File troppo grande (max 5MB)");

            try
            {
                Directory.CreateDirectory(_uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var url = $"/uploads/{fileName}";
                logger.LogInformation("Image uploaded: {FileName} -> {Url}", file.FileName, url);

                return Ok(url);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading image");
                return StatusCode(500, "Errore durante l'upload");
            }
        }

        [HttpDelete]
        public IActionResult Delete(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/"))
                    return BadRequest("URL non valido");

                var fileName = Path.GetFileName(url);
                var filePath = Path.Combine(_uploadsPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    logger.LogInformation("Image deleted: {Url}", url);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting image {Url}", url);
                return StatusCode(500, "Errore durante la cancellazione");
            }
        }
    }
}