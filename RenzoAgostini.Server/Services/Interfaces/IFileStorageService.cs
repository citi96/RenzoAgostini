namespace RenzoAgostini.Server.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, Stream content, string contentType);
    Task<(byte[] Content, string ContentType)?> GetFileAsync(string fileName);
    Task DeleteFileAsync(string fileName);
}
