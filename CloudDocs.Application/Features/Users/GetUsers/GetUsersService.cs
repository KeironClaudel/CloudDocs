using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUsers;

public class GetUsersService : IGetUsersService
{
    private readonly IUserRepository _userRepository;

    public GetUsersService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(x => new UserResponse(
            x.Id,
            x.FullName,
            x.Email,
            x.Department,
            x.Role.Name,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}