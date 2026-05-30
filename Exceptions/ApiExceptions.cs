namespace TutorialProj.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public object? Errors { get; }

    public ApiException(
        string message,
        int statusCode = StatusCodes.Status500InternalServerError,
        object? errors = null,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public sealed class NotFoundException(string message)
    : ApiException(message, StatusCodes.Status404NotFound);

public sealed class ValidationException(string message, object errors)
    : ApiException(message, StatusCodes.Status422UnprocessableEntity, errors);

public sealed class UnauthorizedException(string message)
    : ApiException(message, StatusCodes.Status401Unauthorized);
