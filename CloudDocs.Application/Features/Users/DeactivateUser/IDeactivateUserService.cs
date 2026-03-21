namespace CloudDocs.Application.Features.Users.DeactivateUser;

public interface IDeactivateUserService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}