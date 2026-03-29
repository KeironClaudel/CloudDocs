namespace CloudDocs.Application.Features.Auth.ForgotPassword;

/// <summary>
/// Defines the contract for forgot password operations.
/// </summary>
public interface IForgotPasswordService
{
    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the forgot password response.</returns>
    Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
}