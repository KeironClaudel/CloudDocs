using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.UpdateUser;

/// <summary>
/// Defines the contract for update user operations.
/// </summary>
public interface IUpdateUserService
{
    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response when available; otherwise, null.</returns>
    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
}