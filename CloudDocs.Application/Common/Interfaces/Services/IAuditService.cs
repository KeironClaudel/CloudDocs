namespace CloudDocs.Application.Common.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(
        Guid? userId,
        string action,
        string module,
        string entityName,
        string? entityId = null,
        string? details = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}