using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Services;

/// <summary>
/// Defines the contract for document access operations.
/// </summary>
public interface IDocumentAccessService
{
    /// <summary>
    /// Cans access document.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="document">The document.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    bool CanAccessDocument(User currentUser, Document document);
}