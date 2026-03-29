namespace CloudDocs.Application.Features.Auth.Logout;

/// <summary>
/// Defines the contract for logout operations.
/// </summary>
public interface ILogoutService
{
    /// <summary>
    /// Executes.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}