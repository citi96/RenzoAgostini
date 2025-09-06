using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using Stripe;

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

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = string.Empty;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<RenzoAgostiniDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IPaintingRepository, PaintingRepository>();
builder.Services.AddScoped<PaintingService>();
builder.Services.AddScoped<IPaintingService>(sp =>
    new CachedPaintingService(
        sp.GetRequiredService<PaintingService>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<CachedPaintingService>>()
    ));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// CORS per sviluppo: client e server su origini diverse
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowAnyCorsInDev, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("oauth2",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
        });
});

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.Authority = "http://localhost:8080/realms/RenzoAgostiniRealm";
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/RenzoAgostiniRealm",
            ValidateAudience = true,
            ValidAudiences =
            [
                "web-a1e0f0a5-ed40-4a9f-bd85-87a2273e38f7"
            ],
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        o.MetadataAddress = "http://localhost:8080/realms/RenzoAgostiniRealm/.well-known/openid-configuration";
        o.RequireHttpsMetadata = false; // Solo per sviluppo con Keycloak in HTTP

        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var id = (ClaimsIdentity)ctx.Principal!.Identity!;

                var res = ctx.Principal.FindFirst("resource_access")?.Value;
                if (!string.IsNullOrEmpty(res))
                {
                    var obj = System.Text.Json.JsonDocument.Parse(res);
                    if (obj.RootElement.TryGetProperty("web-a1e0f0a5-ed40-4a9f-bd85-87a2273e38f7", out var client)
                        && client.TryGetProperty("roles", out var arr2))
                        foreach (var r in arr2.EnumerateArray())
                            id.AddClaim(new Claim(ClaimTypes.Role, r.GetString()!));
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
        JwtBearerDefaults.AuthenticationScheme);
    defaultAuthorizationPolicyBuilder =
        defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads",
});

app.Run();
