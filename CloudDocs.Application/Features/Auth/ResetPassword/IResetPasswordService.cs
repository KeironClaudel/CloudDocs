namespace CloudDocs.Application.Features.Auth.ResetPassword;

/// <summary>
/// Defines the contract for reset password operations.
/// </summary>
public interface IResetPasswordService
{
    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}