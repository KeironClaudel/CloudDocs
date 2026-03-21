using CloudDocs.Application.Features.Users.Common;

namespace CloudDocs.Application.Features.Users.CreateUser;

public interface ICreateUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}