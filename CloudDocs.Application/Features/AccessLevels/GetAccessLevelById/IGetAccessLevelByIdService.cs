using CloudDocs.Application.Features.AccessLevels.Common;

namespace CloudDocs.Application.Features.AccessLevels.GetAccessLevelById
{
    public interface IGetAccessLevelByIdService
    {
        Task<AccessLevelResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
