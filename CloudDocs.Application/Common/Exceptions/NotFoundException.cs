namespace CloudDocs.Application.Common.Exceptions;

/// <summary>
/// Represents an exception for not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the bad request.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}