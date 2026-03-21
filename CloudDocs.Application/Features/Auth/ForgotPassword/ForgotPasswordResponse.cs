namespace CloudDocs.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordResponse(
    string Message,
    string? ResetToken);