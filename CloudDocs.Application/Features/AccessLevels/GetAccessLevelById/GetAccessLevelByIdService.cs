using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.GetAccessLevelById;

public class GetAccessLevelByIdService : IGetAccessLevelByIdService
{
    private readonly IAccessLevelRepository _accessLevelRepository;

    public GetAccessLevelByIdService(IAccessLevelRepository accessLevelRepository)
    {
        _accessLevelRepository = accessLevelRepository;
    }

    public async Task<AccessLevelResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var x = await _accessLevelRepository.GetByIdAsync(id, cancellationToken);
        if (x is null) return null;

        return new AccessLevelResponse(
            x.Id,
            x.Code,
            x.Name,
            x.Description,
            x.IsActive,
            x.CreatedAt);
    }
}
