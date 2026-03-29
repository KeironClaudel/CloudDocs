namespace CloudDocs.Application.Features.Auth.ChangePassword;

/// <summary>
/// Defines the contract for change password operations.
/// </summary>
public interface IChangePasswordService
{
    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}