using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TutorialProj.Middleware;
using TutorialProj.Repositories;
using TutorialProj.Repositories.Interfaces;
using TutorialProj.Services.Inventory;
using TutorialProj.Services.Orders;
using Microsoft.AspNetCore.Identity;
using TutorialProj.Models;
using TutorialProj.Services.Auth;
using TutorialProj.Data;
using TutorialProj.Commands;
using TutorialProj.Constants;

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
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

/* Set camelCase format serialization and deserialization for JSON:
Serialization (response output): C# Name → JSON "name"
Deserialization (request input): JSON "name" → C# Name
*/
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
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
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.JwtSecret))
        };
    });

builder.Services.AddAuthorization();

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

// Auth middlewares must come before MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Map Controller routes (e.g. [Route("api/inventory")])
app.MapControllers();

app.Run();
