using System.Text.Json;

namespace DotNetTutorial.Middleware;

/// <summary>
/// Endpoint filter that validates Bearer token authentication.
/// Apply to specific routes using .AddEndpointFilter().
/// </summary>
public class AuthenticationFilter : IEndpointFilter
{
    private const string ValidToken = "mysecrettoken123";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Results.Json(
                new { error = "Unauthorized. Invalid or missing token." },
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var token = authHeader["Bearer ".Length..];

        if (token != ValidToken)
        {
            return Results.Json(
                new { error = "Unauthorized. Invalid or missing token." },
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        return await next(context);
    }
}
