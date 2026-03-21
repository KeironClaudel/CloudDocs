using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUsers;

public interface IGetUsersService
{
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}