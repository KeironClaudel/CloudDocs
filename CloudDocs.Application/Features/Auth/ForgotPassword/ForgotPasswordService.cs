using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await _auditService.LogAsync(
                null,
                "ForgotPasswordRequested",
                "Auth",
                "User",
                null,
                $"Password reset requested for non-existing or inactive email: {request.Email}",
                null,
                cancellationToken);

            return new ForgotPasswordResponse(
                "If the account exists, a password reset link will be sent.",
                null);
        }

        var rawToken = Guid.NewGuid().ToString("N");

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = rawToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            user.Id,
            "ForgotPasswordRequested",
            "Auth",
            "User",
            user.Id.ToString(),
            "Password reset token generated.",
            null,
            cancellationToken);

        return new ForgotPasswordResponse(
            "Password reset token generated successfully.",
            rawToken);
    }
}