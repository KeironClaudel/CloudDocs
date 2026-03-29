namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Defines the contract for audit operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="module">The module.</param>
    /// <param name="entityName">The entity name.</param>
    /// <param name="entityId">The entity id identifier.</param>
    /// <param name="details">The details.</param>
    /// <param name="ipAddress">The ip address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LogAsync(
        Guid? userId,
        string action,
        string module,
        string entityName,
        string? entityId = null,
        string? details = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}