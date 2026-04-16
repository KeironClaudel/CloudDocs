using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Users.Common;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Users.CreateUser;

/// <summary>
/// Provides operations for create user.
/// </summary>
public class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="roleRepository">The role repository.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDepartmentRepository departmentRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _departmentRepository = departmentRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response.</returns>
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new BadRequestException("Email is already in use.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            throw new BadRequestException("Invalid role.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Department = null,
            RoleId = role.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            var deptName = request.Department.Trim();
            var existing = await _departmentRepository.GetByNameAsync(deptName, cancellationToken);
            if (existing is not null)
            {
                user.Department = existing;
            }
            else
            {
                var newDept = new Department { Name = deptName };
                await _departmentRepository.AddAsync(newDept, cancellationToken);
                user.Department = newDept;
            }
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(
                                    null,
                                    "Create",
                                    "Users",
                                    "User",
                                    user.Id.ToString(),
                                    $"User created with email: {user.Email} and role: {role.Name}",
                                    null,
                                    cancellationToken);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Department?.Name,
            role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}