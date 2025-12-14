using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RenzoAgostini.Client;
using RenzoAgostini.Client.Authentication;

using RenzoAgostini.Client.Http.Handlers;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var s1 = await http.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(s1);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<RefreshTokenHandler>();
builder.Services.AddScoped<ErrorMessageHandler>();

// 1. Auth Client (No Handlers - Breaks Circular Dependency)
var baseUrl = builder.Configuration["BaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddHttpClient("AuthClient", client =>
    client.BaseAddress = new Uri(baseUrl));

// 2. API Client (With Handlers - For Protected Resources)
builder.Services.AddHttpClient("ApiClient", client =>
    client.BaseAddress = new Uri(baseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>()
    .AddHttpMessageHandler<RefreshTokenHandler>()
    .AddHttpMessageHandler<ErrorMessageHandler>(); // Added Error Handler here

// 3. Register Default HttpClient to use "ApiClient" (Simplest for existing services)
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

// 4. Register Services
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaintingService, PaintingService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICheckoutService, CheckoutClient>();
builder.Services.AddScoped<IShippingClient, ShippingClient>();
builder.Services.AddScoped<ICustomOrderService, CustomOrderService>();

// 5. Register AuthService specifically with AuthClient
builder.Services.AddScoped<IAuthService>(sp =>
    new AuthService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthClient")));

builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Configurazione autorizzazione
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();