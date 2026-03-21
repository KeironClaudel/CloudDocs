namespace CloudDocs.Application.Features.Auth.ForgotPassword;

public interface IForgotPasswordService
{
    Task<ForgotPasswordResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
}