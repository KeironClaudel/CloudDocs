using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Users.UpdateUser;

/// <summary>
/// Provides operations for update user.
/// </summary>
public class UpdateUserService : IUpdateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="roleRepository">The role repository.</param>
    /// <param name="departmentRepository">The department repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDepartmentRepository departmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _departmentRepository = departmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response when available; otherwise, null.</returns>
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

        Department? department = null;

        if (request.DepartmentId.HasValue)
        {
            department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value, cancellationToken);

            if (department is null || !department.IsActive)
                throw new BadRequestException("Department not found or inactive.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLower();
        user.DepartmentId = department?.Id;
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
            user.DepartmentId,
            department?.Name,
            role.Id,
            role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}