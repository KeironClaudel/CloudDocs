using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Documents.RenameDocument;

/// <summary>
/// Provides operations for rename document.
/// </summary>
public class RenameDocumentService : IRenameDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameDocumentService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public RenameDocumentService(IDocumentRepository documentRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Renames.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> RenameAsync(Guid id, RenameDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
            throw new BadRequestException("New name is required.");

        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return false;

        document.OriginalFileName = request.NewName.Trim();
        document.UpdatedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                    null,
                                    "Rename",
                                    "Documents",
                                    "Document",
                                    document.Id.ToString(),
                                    $"Document renamed to: {document.OriginalFileName}",
                                    null,
                                    cancellationToken);

        return true;
    }
}