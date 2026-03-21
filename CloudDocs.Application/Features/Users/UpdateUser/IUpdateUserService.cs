using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.UpdateUser;

public interface IUpdateUserService
{
    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
}