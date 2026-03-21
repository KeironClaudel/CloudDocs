using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
namespace CloudDocs.Application.Features.Users.DeactivateUser;

public class DeactivateUserService : IDeactivateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    public DeactivateUserService(IUserRepository userRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

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