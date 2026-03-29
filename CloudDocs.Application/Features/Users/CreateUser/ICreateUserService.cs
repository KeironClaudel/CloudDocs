using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.CreateUser;

/// <summary>
/// Defines the contract for create user operations.
/// </summary>
public interface ICreateUserService
{
    /// <summary>
    /// Creates.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response.</returns>
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}