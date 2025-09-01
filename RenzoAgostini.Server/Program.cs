using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Repositories;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Config
var allowAnyCorsInDev = "_allowAnyCorsInDev";

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<RenzoAgostiniDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI dominio
builder.Services.AddScoped<IPaintingRepository, PaintingRepository>();
builder.Services.AddScoped<PaintingService>();
builder.Services.AddScoped<IPaintingService>(sp =>
    new CachedPaintingService(
        sp.GetRequiredService<PaintingService>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<CachedPaintingService>>()
    ));

// CORS per sviluppo: client e server su origini diverse
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowAnyCorsInDev, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(allowAnyCorsInDev);
}

app.UseHttpsRedirection();

// Se servi immagini statiche dal server:
// app.UseStaticFiles(); // e metti le immagini in wwwroot/img

app.MapControllers();

app.Run();
