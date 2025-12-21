using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RenzoAgostini.Server.Auth;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Emailing.Extensions;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Shared.Contracts;
using Stripe;
using System.Security.Claims;
using System.Text;
using IOrderService = RenzoAgostini.Server.Services.Interfaces.IOrderService;

var builder = WebApplication.CreateBuilder(args);

// Config
var allowAnyCorsInDev = "_allowAnyCorsInDev";
var allowConfiguredCors = "_allowConfiguredCors";

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new();
var storageOptions = builder.Configuration.GetSection("Storage").Get<StorageOptions>() ?? new();
var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(o => !string.IsNullOrWhiteSpace(o))
    .Select(o => o.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? Array.Empty<string>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
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
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<RenzoAgostiniDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEmailing(builder.Configuration);

builder.Services.AddSingleton<IClaimsTransformation, LowercaseRoleClaimsTransformation>();

builder.Services.AddSingleton<ITokenService, RenzoAgostini.Server.Services.TokenService>();

builder.Services.AddScoped<IPaintingRepository, PaintingRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomOrderRepository, CustomOrderRepository>();
builder.Services.AddScoped<IShippingOptionRepository, ShippingOptionRepository>();
builder.Services.AddScoped<IBiographyRepository, BiographyRepository>();

// Services
builder.Services.AddScoped<PaintingService>(); // Concrete service for caching
builder.Services.AddScoped<IPaintingService>(sp =>
    new CachedPaintingService(
        sp.GetRequiredService<PaintingService>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<CachedPaintingService>>()
    ));
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomOrderService, CustomOrderService>();
builder.Services.AddScoped<IShippingOptionService, ShippingOptionService>();
builder.Services.AddScoped<IBiographyService, BiographyService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileStorageService, DatabaseFileStorageService>();

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
    o.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
        o.SaveToken = true;
        o.RequireHttpsMetadata = false; // Set to true in production
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
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

    // Seed Roles and Admin
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(RoleNames.Admin)) await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
    if (!await roleManager.RoleExistsAsync(RoleNames.Viewer)) await roleManager.CreateAsync(new IdentityRole(RoleNames.Viewer));

    var adminEmail = "fchiti071@gmail.com"; // Default admin email
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Admin",
            Surname = "User",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        }
    }
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
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
