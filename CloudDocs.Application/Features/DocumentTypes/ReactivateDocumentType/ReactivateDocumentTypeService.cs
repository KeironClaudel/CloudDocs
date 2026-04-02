using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.DocumentTypes.ReactivateDocumentType;

public class ReactivateDocumentTypeService : IReactivateDocumentTypeService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateDocumentTypeService(
        IDocumentTypeRepository documentTypeRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _documentTypeRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = true;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _documentTypeRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Reactivate",
            "DocumentTypes",
            "DocumentType",
            entity.Id.ToString(),
            $"Document type reactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}