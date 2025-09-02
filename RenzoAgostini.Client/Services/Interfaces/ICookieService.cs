namespace RenzoAgostini.Client.Services.Interfaces
{
    public interface ICookieService
    {
        Task ClearAsync(string key);
        Task<T> GetAsync<T>(string key);
        Task PutAsync<T>(string key, T value);
        Task<T> RemoveAsync<T>(string key);
    }
}
