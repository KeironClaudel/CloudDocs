namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves the changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the int.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}