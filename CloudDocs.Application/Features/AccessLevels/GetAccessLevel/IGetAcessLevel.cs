using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.GetAccessLevel;

public interface IGetAccessLevelsService
{
    Task<List<AccessLevelResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}