using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for category persistence operations.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category when available; otherwise, null.</returns>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category list.</returns>
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Determines whether the name exists.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
}