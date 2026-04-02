using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.DocumentTypes.DeactivateDocumentType;

public class DeactivateDocumentTypeService : IDeactivateDocumentTypeService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDocumentTypeService(
        IDocumentTypeRepository documentTypeRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _documentTypeRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _documentTypeRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Deactivate",
            "DocumentTypes",
            "DocumentType",
            entity.Id.ToString(),
            $"Document type deactivated: {entity.Name}",
            null,
            cancellationToken);

        return true;
    }
}