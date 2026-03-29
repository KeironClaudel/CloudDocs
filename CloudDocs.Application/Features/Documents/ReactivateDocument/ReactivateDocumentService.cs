using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Documents.ReactivateDocument;

/// <summary>
/// Provides operations for reactivate document.
/// </summary>
public class ReactivateDocumentService : IReactivateDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactivateDocumentService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public ReactivateDocumentService(
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return false;

        document.IsActive = true;
        document.DeletedAt = null;
        document.DeletedBy = null;
        document.UpdatedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Reactivate",
            "Documents",
            "Document",
            document.Id.ToString(),
            $"Document reactivated: {document.OriginalFileName}{document.FileExtension}",
            null,
            cancellationToken);

        return true;
    }
}
