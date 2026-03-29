using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Auth.ForgotPassword;

/// <summary>
/// Provides operations for forgot password.
/// </summary>
public class ForgotPasswordService : IForgotPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="passwordResetTokenRepository">The password reset token repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
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

    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the forgot password response.</returns>
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