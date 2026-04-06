using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;

public class UpdateDocumentVisibilityService : IUpdateDocumentVisibilityService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateDocumentVisibilityService(
        IDocumentRepository documentRepository,
        IAccessLevelRepository accessLevelRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _documentRepository = documentRepository;
        _accessLevelRepository = accessLevelRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<bool> UpdateAsync(Guid documentId, UpdateDocumentVisibilityRequest request, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null)
            return false;

        var accessLevel = await _accessLevelRepository.GetByIdAsync(request.AccessLevelId, cancellationToken);

        if (accessLevel is null || !accessLevel.IsActive)
            throw new NotFoundException("Access level not found or inactive.");

        var selectedDepartments = new List<Department>();

        if (string.Equals(accessLevel.Code, "DEPARTMENT_ONLY", StringComparison.OrdinalIgnoreCase))
        {
            if (request.DepartmentIds is null || request.DepartmentIds.Count == 0)
                throw new BadRequestException("At least one department must be selected.");

            selectedDepartments = await _departmentRepository.GetByIdsAsync(request.DepartmentIds, cancellationToken);

            if (selectedDepartments.Count != request.DepartmentIds.Distinct().Count())
                throw new BadRequestException("Invalid departments provided.");
        }

        // Actualizar access level
        document.AccessLevelId = accessLevel.Id;

        // Limpiar relaciones anteriores
        document.DocumentDepartments.Clear();

        // Asignar nuevas
        if (selectedDepartments.Count > 0)
        {
            document.DocumentDepartments = selectedDepartments
                .Select(d => new DocumentDepartment
                {
                    DocumentId = document.Id,
                    DepartmentId = d.Id
                })
                .ToList();
        }

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "UpdateVisibility",
            "Documents",
            "Document",
            document.Id.ToString(),
            $"Document visibility updated to {accessLevel.Name}",
            null,
            cancellationToken);

        return true;
    }
}