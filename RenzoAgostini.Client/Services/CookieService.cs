using Microsoft.JSInterop;
using RenzoAgostini.Client.Services.Interfaces;

namespace RenzoAgostini.Client.Services
{
    public class CookieService(IJSRuntime jsRuntime) : ICookieService
    {
        public async Task PutAsync<T>(string key, T value) =>
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);

        public async Task<T> GetAsync<T>(string key) =>
            await jsRuntime.InvokeAsync<T>("sessionStorage.getItem", key);

        public async Task<T> RemoveAsync<T>(string key)
        {
            var value = await GetAsync<T>(key);
            await ClearAsync(key);

            return value;
        }

        public async Task ClearAsync(string key) =>
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, "");
    }
}
