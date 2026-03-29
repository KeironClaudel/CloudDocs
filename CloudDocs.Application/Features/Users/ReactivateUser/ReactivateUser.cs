using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Users.ReactivateUser;

/// <summary>
/// Provides operations for reactivate user.
/// </summary>
public class ReactivateUserService : IReactivateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactivateUserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public ReactivateUserService(IUserRepository userRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the operation succeeded.</returns>
    public async Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return false;

        user.IsActive = true;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(
                                        null,
                                        "Reactivate",
                                        "Users",
                                        "User",
                                        user.Id.ToString(),
                                        $"User reactivated: {user.Email}",
                                        null,
                                        cancellationToken);

        return true;
    }
}