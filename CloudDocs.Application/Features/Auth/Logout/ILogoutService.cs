namespace CloudDocs.Application.Features.Auth.Logout;

public interface ILogoutService
{
    Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}