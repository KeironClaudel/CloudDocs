namespace CloudDocs.Application.Features.AccessLevels.ReactivateAccessLevel;

public interface IReactivateAccessLevelService
{
    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
