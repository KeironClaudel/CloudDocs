using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.AccessLevels.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;

public class CreateAccessLevelService : ICreateAccessLevelService
{
    private readonly IAccessLevelRepository _accessLevelRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccessLevelService(
        IAccessLevelRepository accessLevelRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _accessLevelRepository = accessLevelRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessLevelResponse> CreateAsync(CreateAccessLevelRequest request, CancellationToken cancellationToken = default)
    {
        var codeExists = await _accessLevelRepository.CodeExistsAsync(request.Code, cancellationToken);
        if (codeExists)
            throw new BadRequestException("Access level code is already in use.");

        var nameExists = await _accessLevelRepository.NameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new BadRequestException("Access level name is already in use.");

        var entity = new AccessLevelEntity
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _accessLevelRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            null,
            "Create",
            "AccessLevels",
            "AccessLevel",
            entity.Id.ToString(),
            $"Access level created: {entity.Name} ({entity.Code})",
            null,
            cancellationToken);

        return new AccessLevelResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}
