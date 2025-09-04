using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RenzoAgostini.Client;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var s1 = await http.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(s1);
try
{
    using var s2 = await http.GetStreamAsync($"appsettings.{builder.HostEnvironment.Environment}.json");
    builder.Configuration.AddJsonStream(s2);
}
catch { /* ok se non esiste */ }

// Configurazione HttpClient
var client = builder.Services.AddScoped<HttpClient>(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7215/")
});

// Cache opzionale sul client
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IPaintingService, PaintingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IKeycloakService, KeycloakService>();

builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Configurazione autorizzazione
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();