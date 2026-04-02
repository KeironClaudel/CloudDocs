using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Persistence;

public interface IDocumentTypeRepository
{
    Task<List<DocumentTypeEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DocumentTypeEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(DocumentTypeEntity documentType, CancellationToken cancellationToken = default);
    Task UpdateAsync(DocumentTypeEntity documentType, CancellationToken cancellationToken = default);
}
