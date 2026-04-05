using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.CreateAccessLevel;

public interface ICreateAccessLevelService
{
    Task<AccessLevelResponse> CreateAsync(CreateAccessLevelRequest request, CancellationToken cancellationToken = default);
}