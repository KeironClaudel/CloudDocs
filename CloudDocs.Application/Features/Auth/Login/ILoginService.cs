namespace CloudDocs.Application.Features.Auth.Login;

/// <summary>
/// Defines the contract for login operations.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Logs the in.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the login response.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}