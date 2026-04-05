using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.AccessLevels.Common;
using CloudDocs.Application.Features.AccessLevels.GetAccessLevel;

namespace CloudDocs.Application.Features.AccessLevels.GetAccessLevels;

public class GetAccessLevelsService : IGetAccessLevelsService
{
    private readonly IAccessLevelRepository _accessLevelRepository;

    public GetAccessLevelsService(IAccessLevelRepository accessLevelRepository)
    {
        _accessLevelRepository = accessLevelRepository;
    }

    public async Task<List<AccessLevelResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _accessLevelRepository.GetAllAsync(cancellationToken);

        return items.Select(x => new AccessLevelResponse(
            x.Id,
            x.Code,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}
