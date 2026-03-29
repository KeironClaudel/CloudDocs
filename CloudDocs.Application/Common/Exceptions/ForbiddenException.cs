namespace CloudDocs.Application.Common.Exceptions;

/// <summary>
/// Represents an exception for forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the bad request.</param>
    public ForbiddenException(string message) : base(message)
    {
    }
}