using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RenzoAgostini.Data;
using RenzoAgostini.Repositories;
using RenzoAgostini.Repositories.Interfaces;
using RenzoAgostini.Services;
using RenzoAgostini.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Database
builder.Services.AddDbContext<RenzoAgostiniDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // oppure per development con SQLite:
    // options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repository Pattern
builder.Services.AddScoped<IPaintingRepository, PaintingRepository>();

// Service Layer
builder.Services.AddScoped<PaintingService>();
builder.Services.AddScoped<IPaintingService>(provider =>
{
    var innerService = provider.GetService<PaintingService>()!;
    var cache = provider.GetService<IMemoryCache>()!;
    var logger = provider.GetService<ILogger<CachedPaintingService>>()!;

    return new CachedPaintingService(innerService, cache, logger);
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RenzoAgostiniDbContext>();

// Caching (opzionale per performance)
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHealthChecks("/health");

// Database migration in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RenzoAgostiniDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();
