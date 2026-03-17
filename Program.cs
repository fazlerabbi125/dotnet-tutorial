using DotNetTutorial.Middleware;
using System.Text.Json;
using System.Text; // Required for Encoding
using DotNetEnv;
using Microsoft.Data.Sqlite;
using DotNetTutorial.Repositories;
using DotNetTutorial.Validation;
using DotNetTutorial.Models;
using DotNetTutorial.Services; // Required for AuthService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required for JwtBearerDefaults
using Microsoft.IdentityModel.Tokens; // Required for TokenValidationParameters, SymmetricSecurityKey
Env.Load();

var builder = WebApplication.CreateBuilder(args); // Creates a WebApplicationBuilder 

/* Set camelCase format serialization and deserialization for JSON:
Serialization (response output): C# Name → JSON "name"
Deserialization (request input): JSON "name" → C# Name
*/
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "fallback_secret_key_needs_to_be_long_enough"))
    };
});


string connectionString = $"Data Source={Environment.GetEnvironmentVariable("DB_NAME")}";

DatabaseInitializer.Initialize(connectionString);
builder.Services.AddScoped(sp => new UserRepository(connectionString));
builder.Services.AddScoped(sp => new RefreshTokenRepository(connectionString));
builder.Services.AddScoped<AuthService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Document name is v1
builder.Services.AddControllers(); // configures the MVC services for the commonly used features with controllers for an API, excluding views/pages
builder.Services.AddAuthorization();

var app = builder.Build(); // creates a WebApplications

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // https://localhost:{port}/openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API V1");
    });
}

// Middleware pipeline order (Onion Architecture):
// 1. Error handling (catches all unhandled exceptions)
// 2. Authentication (blocks unauthorized requests)
// 3. Authorization (blocks unauthenticated requests or unauthorized roles)
// 4. Logging (logs all requests and responses)
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();

app.MapGet("/users", async (UserRepository repo) =>
{
    var users = await repo.GetAll();
    return Results.Ok(users);
});

app.MapGet("/users/{id}", async (int id, UserRepository repo) =>
{
    var user = await repo.GetOne(id);
    if (user == null)
    {
        return Results.NotFound(new { error = $"User with id {id} not found." });
    }
    return Results.Ok(user);
});

app.MapPost("/submit", async ([FromForm] string username, [FromForm] string email, UserRepository repo) =>
{
    // ... logic remains same as register/login ...
    return Results.BadRequest(new { message = "Please use /api/auth/register instead" });
});

app.MapPut("/users/{id}", async (int id, [FromBody] UserUpdateSchema userUpdate, UserRepository repo) =>
{
    var errors = DataValidator.ValidateSchema(userUpdate);
    if (errors.Count > 0) return Results.BadRequest(errors);

    var existingUser = await repo.GetOne(id);
    if (existingUser == null) return Results.NotFound(new { error = $"User with id {id} not found." });

    if (userUpdate.Username != null && userUpdate.Username != existingUser.Username)
    {
        var duplicate = await repo.GetByUsername(userUpdate.Username);
        if (duplicate != null) return Results.Conflict(new { message = "Username already exists" });
    }

    var updatedUser = await repo.UpdateUser(id, userUpdate);
    return Results.Ok(updatedUser);
}).RequireAuthorization(); // Requires any logged-in user

app.MapDelete("/users/{id}", async (int id, UserRepository repo) =>
{
    var existingUser = await repo.GetOne(id);
    if (existingUser == null) return Results.NotFound(new { error = $"User with id {id} not found." });

    await repo.DeleteUser(id);
    return Results.NoContent();
}).RequireAuthorization(policy => policy.RequireRole("Admin")); // Requires Admin role

app.Run();
