using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;

namespace CloudDocs.Application.Features.Users.ReactivateUser;

public class ReactivateUserService : IReactivateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateUserService(IUserRepository userRepository, IAuditService auditService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

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