using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines the contract for document version persistence operations.
/// </summary>
public interface IDocumentVersionRepository
{
    /// <summary>
    /// Adds.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the item by document id.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document version list.</returns>
    Task<List<DocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the next version number.
    /// </summary>
    /// <param name="documentId">The document id identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the int.</returns>
    Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default);
}