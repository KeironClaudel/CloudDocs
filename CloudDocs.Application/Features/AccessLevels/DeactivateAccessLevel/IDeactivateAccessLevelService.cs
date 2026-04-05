namespace CloudDocs.Application.Features.AccessLevels.DeactivateAccessLevel;

public interface IDeactivateAccessLevelService
{
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
