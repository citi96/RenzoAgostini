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
using RenzoAgostini.Server.Emailing.Extensions;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Contracts;
using Stripe;
using System.Security.Claims;
using IOrderService = RenzoAgostini.Server.Services.Interfaces.IOrderService;

var builder = WebApplication.CreateBuilder(args);

// Config
var allowAnyCorsInDev = "_allowAnyCorsInDev";
var allowConfiguredCors = "_allowConfiguredCors";

var keycloakOptions = builder.Configuration.GetSection("Keycloak").Get<KeycloakOptions>() ?? new();
var storageOptions = builder.Configuration.GetSection("Storage").Get<StorageOptions>() ?? new();
var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(o => !string.IsNullOrWhiteSpace(o))
    .Select(o => o.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? Array.Empty<string>();

builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

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

builder.Services.AddEmailing(builder.Configuration);

builder.Services.AddScoped<IPaintingRepository, PaintingRepository>();
builder.Services.AddScoped<PaintingService>();
builder.Services.AddScoped<IPaintingService>(sp =>
    new CachedPaintingService(
        sp.GetRequiredService<PaintingService>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<CachedPaintingService>>()
    ));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomOrderRepository, CustomOrderRepository>();
builder.Services.AddScoped<ICustomOrderService, CustomOrderService>();
builder.Services.AddScoped<IShippingOptionRepository, ShippingOptionRepository>();
builder.Services.AddScoped<IShippingOptionService, ShippingOptionService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
var stripeApiKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeApiKey))
{
    StripeConfiguration.ApiKey = stripeApiKey;
}

// CORS per sviluppo: client e server su origini diverse
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowAnyCorsInDev, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });

    if (configuredOrigins.Length > 0)
    {
        options.AddPolicy(name: allowConfiguredCors, policy =>
        {
            policy.WithOrigins(configuredOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
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
        var authority = keycloakOptions.Authority?.TrimEnd('/');
        var audiences = keycloakOptions.Audiences
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        if (!audiences.Any() && !string.IsNullOrWhiteSpace(keycloakOptions.ClientId))
        {
            audiences.Add(keycloakOptions.ClientId);
        }

        if (!string.IsNullOrWhiteSpace(authority))
        {
            o.Authority = authority;
            o.MetadataAddress = $"{authority}/.well-known/openid-configuration";
        }

        o.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(authority),
            ValidIssuer = authority,
            ValidateAudience = audiences.Any(),
            ValidAudiences = audiences,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var id = (ClaimsIdentity)ctx.Principal!.Identity!;

                var res = ctx.Principal.FindFirst("resource_access")?.Value;
                var clientId = keycloakOptions.ClientId;

                if (!string.IsNullOrEmpty(res) && !string.IsNullOrWhiteSpace(clientId))
                {
                    var obj = System.Text.Json.JsonDocument.Parse(res);
                    if (obj.RootElement.TryGetProperty(clientId, out var client)
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RenzoAgostiniDbContext>();
    db.Database.Migrate();
}

var uploadsRoot = ResolvePath(storageOptions.UploadsPath, builder.Environment);
var customOrdersRoot = ResolvePath(storageOptions.CustomOrdersPath, builder.Environment);

if (!string.IsNullOrWhiteSpace(uploadsRoot))
{
    Directory.CreateDirectory(uploadsRoot);
}

if (!string.IsNullOrWhiteSpace(customOrdersRoot))
{
    Directory.CreateDirectory(customOrdersRoot);
}

// Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(allowAnyCorsInDev);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(allowAnyCorsInDev);
}
else if (configuredOrigins.Length > 0)
{
    app.UseCors(allowConfiguredCors);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseStaticFiles();
if (!string.IsNullOrWhiteSpace(uploadsRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });
}

app.Run();

static string? ResolvePath(string? configuredPath, IWebHostEnvironment environment)
{
    if (string.IsNullOrWhiteSpace(configuredPath))
    {
        return null;
    }

    if (Path.IsPathRooted(configuredPath))
    {
        return configuredPath;
    }

    return Path.Combine(environment.ContentRootPath, configuredPath);
}
