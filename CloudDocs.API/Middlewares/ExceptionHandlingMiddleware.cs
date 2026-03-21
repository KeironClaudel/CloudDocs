using System.Net;
using System.Text.Json;
using CloudDocs.Application.Common.Exceptions;

namespace CloudDocs.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

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