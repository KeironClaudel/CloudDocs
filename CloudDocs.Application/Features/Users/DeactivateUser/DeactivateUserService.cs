using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
namespace CloudDocs.Application.Features.Users.DeactivateUser;

/// <summary>
/// Provides operations for deactivate user.
/// </summary>
public class DeactivateUserService : IDeactivateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// Initializes a new instance of the <see cref="DeactivateUserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeactivateUserService(IUserRepository userRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return false;

        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                    null,
                                    "Deactivate",
                                    "Users",
                                    "User",
                                    user.Id.ToString(),
                                    $"User deactivated: {user.Email}",
                                    null,
                                    cancellationToken);

        return true;
    }
}