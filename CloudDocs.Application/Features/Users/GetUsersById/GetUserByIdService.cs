using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUserById;

/// <summary>
/// Provides operations for get user by id.
/// </summary>
public class GetUserByIdService : IGetUserByIdService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserByIdService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public GetUserByIdService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response when available; otherwise, null.</returns>
    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return null;

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.DepartmentId,
            user.Department?.Name,
            user.Role.Id,
            user.Role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}