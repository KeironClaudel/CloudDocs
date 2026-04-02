using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.DocumentTypes.Common;

namespace CloudDocs.Application.Features.DocumentTypes.UpdateDocumentType;

public class UpdateDocumentTypeService : IUpdateDocumentTypeService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentTypeService(
        IDocumentTypeRepository documentTypeRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentTypeResponse?> UpdateAsync(Guid id, UpdateDocumentTypeRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _documentTypeRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var exists = await _documentTypeRepository.NameExistsAsync(request.Name, cancellationToken);
        var normalizedCurrent = entity.Name.Trim().ToLower();
        var normalizedRequested = request.Name.Trim().ToLower();

        if (exists && normalizedCurrent != normalizedRequested)
            throw new BadRequestException("Document type name is already in use.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _documentTypeRepository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Update",
            "DocumentTypes",
            "DocumentType",
            entity.Id.ToString(),
            $"Document type updated: {entity.Name}",
            null,
            cancellationToken);

        return new DocumentTypeResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}