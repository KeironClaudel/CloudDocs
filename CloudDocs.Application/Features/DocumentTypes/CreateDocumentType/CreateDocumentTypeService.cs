using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.DocumentTypes.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.DocumentTypes.CreateDocumentType;

public class CreateDocumentTypeService : ICreateDocumentTypeService
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDocumentTypeService(
        IDocumentTypeRepository documentTypeRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentTypeResponse> CreateAsync(CreateDocumentTypeRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _documentTypeRepository.NameExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new BadRequestException("Document type name is already in use.");

        var entity = new DocumentTypeEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _documentTypeRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Create",
            "DocumentTypes",
            "DocumentType",
            entity.Id.ToString(),
            $"Document type created: {entity.Name}",
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