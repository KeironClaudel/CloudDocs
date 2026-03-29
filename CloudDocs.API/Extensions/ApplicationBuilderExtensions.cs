using CloudDocs.API.Middleware;

namespace CloudDocs.API.Extensions;

/// <summary>
/// Provides extension methods for application builder.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the global exception handling.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <returns>The i application builder.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}