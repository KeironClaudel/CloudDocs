using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Helpers;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Auth.ChangePassword;

public class ChangePasswordService : IChangePasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new NotFoundException("User not found or inactive.");

        var isCurrentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
            throw new BadRequestException("Current password is incorrect.");

        if (!PasswordRules.IsValid(request.NewPassword))
            throw new BadRequestException("New password does not meet security requirements.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            user.Id,
            "PasswordChanged",
            "Auth",
            "User",
            user.Id.ToString(),
            "User changed password successfully.",
            null,
            cancellationToken);
    }
}