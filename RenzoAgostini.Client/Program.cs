using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RenzoAgostini.Client;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Shared.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7215/")
});

// Cache opzionale sul client
builder.Services.AddMemoryCache();

// Proxy client che chiama le API del server
builder.Services.AddScoped<IPaintingService, PaintingService>();

await builder.Build().RunAsync();

