using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.GetUserById;

public interface IGetUserByIdService
{
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}