using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Services.Interfaces;

namespace RenzoAgostini.Server.Services;

public class DatabaseFileStorageService(RenzoAgostiniDbContext context, ILogger<DatabaseFileStorageService> logger) : IFileStorageService
{
    public async Task<string> SaveFileAsync(string fileName, Stream content, string contentType)
    {
        // Read stream to byte array
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var storedFile = new StoredFile
        {
            FileName = fileName,
            ContentType = contentType,
            Content = fileBytes
        };

        context.StoredFiles.Add(storedFile);
        await context.SaveChangesAsync();

        logger.LogInformation("File {FileName} saved to database. Size: {Size} bytes", fileName, fileBytes.Length);

        // Return the virtual path that the controller will serve
        return $"/api/images/{fileName}";
    }

    public async Task<(byte[] Content, string ContentType)?> GetFileAsync(string fileName)
    {
        var file = await context.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileName == fileName);

        if (file == null) return null;

        return (file.Content, file.ContentType);
    }

    public async Task DeleteFileAsync(string fileName)
    {
        // Must extract just the filename if a full URL or path was passed
        // Assuming fileName passed here matches what's in DB
        // If the caller passes "/api/images/guid.jpg", we need to strip prefix.
        // However, the interface expects just the identifier usually. 
        // Let's assume the controller handles parsing or we strip here to be safe.

        var cleanName = Path.GetFileName(fileName);

        var file = await context.StoredFiles
            .FirstOrDefaultAsync(f => f.FileName == cleanName);

        if (file != null)
        {
            context.StoredFiles.Remove(file);
            await context.SaveChangesAsync();
            logger.LogInformation("File {FileName} deleted from database", cleanName);
        }
    }
}
