namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
        Task DeleteImageAsync(string imageUrl);
    }
}
