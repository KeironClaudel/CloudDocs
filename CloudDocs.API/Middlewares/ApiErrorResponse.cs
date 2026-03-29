namespace CloudDocs.API.Middleware;

/// <summary>
/// Represents a standardized API error response.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the details.
    /// </summary>
    public string? Details { get; set; }
}