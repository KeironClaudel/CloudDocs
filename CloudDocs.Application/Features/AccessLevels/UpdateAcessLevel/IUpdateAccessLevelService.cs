using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.UpdateAccessLevel;

public interface IUpdateAccessLevelService
{
    Task<AccessLevelResponse?> UpdateAsync(Guid id, UpdateAccessLevelRequest request, CancellationToken cancellationToken = default);
}