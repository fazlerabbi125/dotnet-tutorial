namespace dotnet_tutorial.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("[Request] {Method} {Path}", method, path);

        await _next(context);

        _logger.LogInformation("[Response] {Method} {Path} — {StatusCode}", method, path, context.Response.StatusCode);
    }
}
