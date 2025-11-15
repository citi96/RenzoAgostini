using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RenzoAgostini.Client;
using RenzoAgostini.Client.Authentication;
using RenzoAgostini.Client.Http;
using RenzoAgostini.Client.Http.Handlers;
using RenzoAgostini.Client.Services;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var s1 = await http.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(s1);
//var view = ((IConfigurationRoot)builder.Configuration).GetDebugView();
//Console.WriteLine(view);

//try
//{
//    using var s2 = await http.GetStreamAsync($"appsettings.{builder.HostEnvironment.Environment}.json");
//    builder.Configuration.AddJsonStream(s2);
//    view = ((IConfigurationRoot)builder.Configuration).GetDebugView();
//    Console.WriteLine(view);

//}
//catch { /* ok se non esiste */ }

builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<RefreshTokenHandler>();
builder.Services.AddScoped<ErrorMessageHandler>();
builder.Services.AddScoped<HttpClient, CustomHttpClient>();

// Cache opzionale sul client
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IPaintingService, PaintingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICheckoutService, CheckoutClient>();
builder.Services.AddScoped<IShippingClient, ShippingClient>();
builder.Services.AddScoped<ICustomOrderService, CustomOrderService>();

builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Configurazione autorizzazione
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();