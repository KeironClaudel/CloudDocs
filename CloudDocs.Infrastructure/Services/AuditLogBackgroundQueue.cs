using System.Threading.Channels;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudDocs.Infrastructure.Services;

/// <summary>
/// Queues audit logs and persists them in the background.
/// </summary>
public sealed class AuditLogBackgroundQueue : BackgroundService, IAuditLogQueue
{
    private readonly Channel<AuditLogRequest> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogBackgroundQueue> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogBackgroundQueue"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory.</param>
    /// <param name="logger">The logger.</param>
    public AuditLogBackgroundQueue(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogBackgroundQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _channel = Channel.CreateUnbounded<AuditLogRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <summary>
    /// Queues an audit log request for background processing.
    /// </summary>
    /// <param name="request">The audit log request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public ValueTask QueueAsync(AuditLogRequest request, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(request, cancellationToken);
    }

    /// <summary>
    /// Processes queued audit log requests.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

                await auditService.LogAsync(
                    request.UserId,
                    request.Action,
                    request.Module,
                    request.EntityName,
                    request.EntityId,
                    request.Details,
                    request.IpAddress,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist queued audit log for action {Action}.", request.Action);
            }
        }
    }
}
