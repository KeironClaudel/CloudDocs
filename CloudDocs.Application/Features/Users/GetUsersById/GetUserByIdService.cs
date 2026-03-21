using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUserById;

public class GetUserByIdService : IGetUserByIdService
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return null;

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Department,
            user.Role.Name,
            user.IsActive,
            user.CreatedAt);
    }
}