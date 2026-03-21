namespace CloudDocs.Application.Features.Users.ReactivateUser;

public interface IReactivateUserService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}