using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Exceptions;

namespace CloudDocs.Application.Features.Users.UpdateUser;

public class UpdateUserService : IUpdateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return null;

        var emailInUse = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (emailInUse is not null && emailInUse.Id != id)
            throw new BadRequestException("Email is already in use.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            throw new NotFoundException("Role not found.");

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLower();
        user.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
        user.RoleId = role.Id;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                        null,
                                        "Update",
                                        "Users",
                                        "User",
                                        user.Id.ToString(),
                                        $"User updated. New email: {user.Email}, role: {role.Name}",
                                        null,
                                        cancellationToken);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Department,
            role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}