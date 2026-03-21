using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Features.Users.Common;
using CloudDocs.Domain.Entities;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Exceptions;

namespace CloudDocs.Application.Features.Users.CreateUser;

public class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    public CreateUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new BadRequestException("Email is already in use.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            throw new BadRequestException("Email is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            RoleId = role.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

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
            user.Department,
            role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}