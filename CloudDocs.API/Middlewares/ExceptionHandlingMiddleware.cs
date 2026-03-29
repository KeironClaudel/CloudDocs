using System.Net;
using System.Text.Json;
using CloudDocs.Application.Common.Exceptions;

namespace CloudDocs.API.Middleware;

/// <summary>
/// Handles exception handling in the HTTP request pipeline.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <param name="environment">The environment.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Invokes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _environment, _logger);
        }
    }


    /// <summary>
    /// Handles exceptions globally and converts them into standardized JSON HTTP responses.
    /// Maps known exception types to appropriate status codes, logs the error,
    /// and optionally includes detailed information in development environments.
    /// </summary>
    /// <param name="context">The current HTTP context containing request and response information.</param>
    /// <param name="exception">The exception that was thrown during request processing.</param>
    /// <param name="environment">The hosting environment used to determine if detailed error information should be included.</param>
    /// <param name="logger">The logger used to record warning and error details.</param>
    /// <returns>A task that represents the asynchronous write operation to the HTTP response.</returns>
    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ApiErrorResponse();

        switch (exception)
        {
            case BadRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = exception.Message;
                logger.LogWarning(
                    exception,
                    "Bad request at {Method} {Path}. Message: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
                break;

            case UnauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = exception.Message;
                logger.LogWarning(
                    exception,
                    "Unauthorized access at {Method} {Path}. Message: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
                break;

            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = exception.Message;
                logger.LogWarning(
                    exception,
                    "Forbidden access at {Method} {Path}. Message: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
                break;

            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = exception.Message;
                logger.LogWarning(
                    exception,
                    "Resource not found at {Method} {Path}. Message: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = "An unexpected error occurred.";
                logger.LogError(
                    exception,
                    "Unhandled exception at {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                break;
        }

        if (environment.IsDevelopment())
        {
            errorResponse.Details = exception.Message;
        }

        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(json);
    }
}