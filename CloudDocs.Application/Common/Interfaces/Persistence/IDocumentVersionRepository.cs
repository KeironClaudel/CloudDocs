using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface IDocumentVersionRepository
{
    Task AddAsync(DocumentVersion version, CancellationToken cancellationToken = default);
    Task<List<DocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default);
}