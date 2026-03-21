namespace CloudDocs.Application.Features.Auth.ResetPassword;

public interface IResetPasswordService
{
    Task ExecuteAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}