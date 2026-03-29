namespace CloudDocs.Application.Common.Exceptions;

/// <summary>
/// Represents an exception for unauthorized.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the bad request.</param>
    public UnauthorizedException(string message) : base(message)
    {
    }
}