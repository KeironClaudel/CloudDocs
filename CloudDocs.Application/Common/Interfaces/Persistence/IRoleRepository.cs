using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for role persistence operations.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role when available; otherwise, null.</returns>
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}