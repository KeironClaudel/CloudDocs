using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;
using CloudDocs.Infrastructure.Persistence;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Provides operations for audit.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Logs the action made by user.
    /// </summary>
    /// <param name="userId">The user id identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="module">The module.</param>
    /// <param name="entityName">The entity name.</param>
    /// <param name="entityId">The entity id identifier.</param>
    /// <param name="details">The details.</param>
    /// <param name="ipAddress">The ip address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogAsync(
        Guid? userId,
        string action,
        string module,
        string entityName,
        string? entityId = null,
        string? details = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Module = module,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}