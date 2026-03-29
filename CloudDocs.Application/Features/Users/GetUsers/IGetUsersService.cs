using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUsers;

/// <summary>
/// Defines the contract for get users operations.
/// </summary>
public interface IGetUsersService
{
    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response list.</returns>
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}