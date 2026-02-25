using System.Text.Json;

namespace UserManagementAPI.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ValidToken = "mysecrettoken123";

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Skip authentication for Swagger and OpenAPI endpoints
        if (path.StartsWith("/swagger") || path.StartsWith("/openapi"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new { error = "Unauthorized. Invalid or missing token." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        var token = authHeader["Bearer ".Length..];

        if (token != ValidToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new { error = "Unauthorized. Invalid or missing token." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _next(context);
    }
}
