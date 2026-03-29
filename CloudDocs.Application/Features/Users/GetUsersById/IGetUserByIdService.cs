using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUserById;

/// <summary>
/// Defines the contract for get user by id operations.
/// </summary>
public interface IGetUserByIdService
{
    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response when available; otherwise, null.</returns>
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}