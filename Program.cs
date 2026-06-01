using System.Text.Json;
using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TutorialProj.Middleware;
using TutorialProj.Repositories;
using TutorialProj.Repositories.Interfaces;
using TutorialProj.Services.Inventory;
using TutorialProj.Services.Orders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TutorialProj.Models;
using TutorialProj.Services.Auth;
using TutorialProj.Data;
using TutorialProj.Commands;
using TutorialProj.Common;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args); // Creates a WebApplicationBuilder

// Register AppConfig as a Singleton (creates one instance for the entire app lifetime)
var appConfig = new AppConfig();
builder.Services.AddSingleton(appConfig);

// Register DbContext with connection string from AppConfig
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(appConfig.DbConnectionString);
});

// Configure ASP.NET Core Identity for authentication and authorization
// AddIdentityCore prevents default cookie authentication from interfering with our JWT setup.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Lockout policy: 5 failed attempts → 5-minute lockout.
        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration#lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Replace Identity's default PBKDF2 password hasher with Argon2id.
builder.Services.Replace(
    ServiceDescriptor.Scoped<IPasswordHasher<ApplicationUser>, Argon2idPasswordHasher<ApplicationUser>>());

/* Set camelCase format serialization and deserialization for JSON:
Serialization (response output): C# Name → JSON "name"
Deserialization (request input): JSON "name" → C# Name
*/
// For Minimal APIs
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
// });

// MVC controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Register JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            // Issuer/audience are intentionally not configured in this tutorial;
            // tokens are minted in-process with null issuer/audience (see AuthService).
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.JwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Rate limiting for sensitive endpoints (e.g. login).
// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    rateLimiterOptions.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Add In-Memory Caching (Used by InventoryService)
builder.Services.AddMemoryCache();

// Register Repositories and Services for Dependency Injection (DI)
// Scoped means a new instance is created once per HTTP request.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CreateAdminUserCommand>();

// Configure OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

var isCreateAdminCommand = args.Any(arg =>
    string.Equals(arg, AppCommands.CreateAdmin, StringComparison.OrdinalIgnoreCase));

if (isCreateAdminCommand)
{
    await DbSeeder.SeedAsync(app.Services, seedAdminFromEnvironment: false);

    using var scope = app.Services.CreateScope();
    var createAdminUserCommand = scope.ServiceProvider.GetRequiredService<CreateAdminUserCommand>();
    await createAdminUserCommand.ExecuteAsync();
    return;
}

// Run Database Seeder
await DbSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API V1");
    });
}

// Middleware pipeline order
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseRateLimiter();

// Auth middlewares must come before MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Map Controller routes (e.g. [Route("api/inventory")])
app.MapControllers();

app.Run();
