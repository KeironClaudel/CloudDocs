using CloudDocs.Application.Common.Models;

namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Defines the contract for queuing audit log operations.
/// </summary>
public interface IAuditLogQueue
{
    /// <summary>
    /// Queues an audit log request for background processing.
    /// </summary>
    /// <param name="request">The audit log request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask QueueAsync(AuditLogRequest request, CancellationToken cancellationToken = default);
}
