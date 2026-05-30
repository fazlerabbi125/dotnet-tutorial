using System.Text.Json; // Required for JsonNamingPolicy
using System.Text; // Required for Encoding.UTF8
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required for JwtBearerDefaults
using Microsoft.IdentityModel.Tokens; // Required for TokenValidationParameters, SymmetricSecurityKey
using TutorialProj.Middleware;
using TutorialProj.Common;
using TutorialProj.Models;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args); // Creates a WebApplicationBuilder

// Register AppConfig as a Singleton (creates one instance for the entire app lifetime)
var appConfig = new AppConfig();
builder.Services.AddSingleton(appConfig);

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
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.JwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Add In-Memory Caching
builder.Services.AddMemoryCache();

// Configure OpenAPI/Swagger
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers(); // configures the MVC services for the commonly used features with controllers for an API, excluding views/pages

var app = builder.Build(); // creates a WebApplications

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // https://{host}:{port}/openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API V1");
    });
}

// Middleware pipeline order uses Onion Architecture, where the outer layers (like error handling) wrap around the inner layers (like authentication and routing) and inner layers can handle requests or propagate exceptions to the outer layers.

app.UseMiddleware<ErrorHandlingMiddleware>();

// Auth middlewares must come before MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Map Controller routes
app.MapControllers();

app.Run();

