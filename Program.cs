using DotNetTutorial.Middleware;
using System.Text.Json;
using DotNetEnv;
using Microsoft.Data.Sqlite;
using DotNetTutorial.Repositories;
using DotNetTutorial.Validation;
using DotNetTutorial.Models;
using Microsoft.AspNetCore.Mvc;

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

string connectionString = $"Data Source={Environment.GetEnvironmentVariable("DB_NAME")}";

DatabaseInitializer.Initialize(connectionString);
builder.Services.AddScoped(sp => new UserRepository(connectionString));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Document name is v1
builder.Services.AddControllers(); // configures the MVC services for the commonly used features with controllers for an API, excluding views/pages

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
// 3. Logging (logs all requests and responses)
app.UseMiddleware<ErrorHandlingMiddleware>();
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
    // 1. Manually map form fields to the Schema for validation
    var data = new UserCreateSchema
    {
        Username = username,
        Email = email
    };

    // 2. SANITIZATION 
    data.Username = DataValidator.Sanitize(data.Username);
    data.Email = DataValidator.Sanitize(data.Email);

    // 3. VALIDATION
    var errors = DataValidator.ValidateSchema(data);
    if (errors.Count > 0)
    {
        // For a web form, you might return a View, 
        // but for this API-style setup, we return the error list.
        return Results.BadRequest(new { errors });
    }

    // 4. SECURE INSERTION
    User newUser = await repo.InsertUser(data);

    // Redirect or return success
    return Results.Created($"/users/{newUser.UserID}", new { message = "User created successfully!", id = newUser.UserID });
});

app.Run();
