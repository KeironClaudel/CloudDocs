namespace CloudDocs.Application.Features.Auth.ChangePassword;

public interface IChangePasswordService
{
    Task ExecuteAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}