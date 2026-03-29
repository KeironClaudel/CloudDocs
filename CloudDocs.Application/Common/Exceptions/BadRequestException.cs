namespace CloudDocs.Application.Common.Exceptions;

/// <summary>
/// Represents an exception for bad request.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class
    /// </summary>
    /// <param name="message">The error message that describes the bad request.</param>
    /// <returns>The bad request exception(stringmessage):.</returns>
    public BadRequestException(string message) : base(message)
    {
    }
}