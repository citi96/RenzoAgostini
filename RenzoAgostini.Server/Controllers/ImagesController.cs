using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Constants;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController(IFileStorageService fileStorage, ILogger<ImagesController> logger) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nessun file selezionato");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Tipo di file non supportato");

            const long maxSize = 10 * 1024 * 1024; // Increased to 10MB for DB storage if needed
            if (file.Length > maxSize)
                return BadRequest("File troppo grande (max 10MB)");

            try
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using var stream = file.OpenReadStream();
                var url = await fileStorage.SaveFileAsync(fileName, stream, file.ContentType);

                logger.LogInformation("Image uploaded to DB: {FileName} -> {Url}", file.FileName, url);

                return Ok(url);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading image");
                return StatusCode(500, "Errore durante l'upload");
            }
        }

        [HttpGet("{fileName}")]
        [AllowAnonymous] // Public access to images
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var result = await fileStorage.GetFileAsync(fileName);
                if (result == null)
                    return NotFound();

                return File(result.Value.Content, result.Value.ContentType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving image {FileName}", fileName);
                return StatusCode(500, "Errore durante il recupero dell'immagine");
            }
        }

        [HttpDelete]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Delete(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return BadRequest("URL non valido");

                var fileName = Path.GetFileName(url);
                await fileStorage.DeleteFileAsync(fileName);

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
