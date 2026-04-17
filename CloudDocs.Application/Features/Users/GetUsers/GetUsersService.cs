using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUsers;

/// <summary>
/// Provides operations for get users.
/// </summary>
public class GetUsersService : IGetUsersService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public GetUsersService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user response list.</returns>
    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(x => new UserResponse(
            x.Id,
            x.FullName,
            x.Email,
            x.DepartmentId,
            x.Department?.Name,
            x.Role.Id,
            x.Role.Name,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}