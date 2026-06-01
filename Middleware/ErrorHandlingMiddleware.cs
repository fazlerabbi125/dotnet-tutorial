using System.Text.Json;
using TutorialProj.Exceptions;

namespace TutorialProj.Middleware;

public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Cannot rewrite the response once headers have been flushed.
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception thrown after response started; cannot write error envelope.");
                throw;
            }

            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = MapException(ex);

        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning(ex, "Client error on {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, ex.Message);

        var body = new
        {
            message,
            path = context.Request.Path.Value ?? string.Empty,
            errors,
            timeStamp = DateTime.UtcNow.ToString("o")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private (int statusCode, string message, object? errors) MapException(Exception ex) => ex switch
    {
        ApiException api => (api.StatusCode, api.Message, api.Errors),
        BadHttpRequestException bad => (StatusCodes.Status400BadRequest, bad.Message, null),
        ArgumentException arg => (StatusCodes.Status422UnprocessableEntity, arg.Message, null),
        _ => (
            StatusCodes.Status500InternalServerError,
            _env.IsDevelopment() ? ex.Message : "Internal server error.",
            null
        )
    };
}
