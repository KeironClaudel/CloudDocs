using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Helpers;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Auth.ResetPassword;

public class ResetPasswordService : IResetPasswordService
{
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordService(
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!PasswordRules.IsValid(request.NewPassword))
            throw new BadRequestException("New password does not meet security requirements.");

        var resetToken = await _passwordResetTokenRepository.GetValidTokenAsync(request.Token, cancellationToken);

        if (resetToken is null)
            throw new BadRequestException("Invalid or expired token.");

        var user = resetToken.User;

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        resetToken.UsedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            user.Id,
            "PasswordResetCompleted",
            "Auth",
            "User",
            user.Id.ToString(),
            "Password reset completed successfully.",
            null,
            cancellationToken);
    }
}