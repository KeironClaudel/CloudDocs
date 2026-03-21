using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Documents.DeactivateDocument;

public class DeactivateDocumentService : IDeactivateDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    public DeactivateDocumentService(IDocumentRepository documentRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return false;

        document.IsActive = false;
        document.DeletedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                        null,
                                        "Deactivate",
                                        "Documents",
                                        "Document",
                                        document.Id.ToString(),
                                        $"Document deactivated: {document.OriginalFileName}",
                                        null,
                                        cancellationToken);

        return true;
    }
}