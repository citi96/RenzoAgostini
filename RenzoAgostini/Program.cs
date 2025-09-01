using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RenzoAgostini;
using RenzoAgostini.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddDbContext<RenzoAgostiniDbContext>(o => 
    o.UseSqlite(builder.Configuration.GetConnectionString("DocetConnectionString")), ServiceLifetime.Scoped);

await builder.Build().RunAsync();
