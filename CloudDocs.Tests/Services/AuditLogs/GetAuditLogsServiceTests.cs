using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Models;
using CloudDocs.Application.Features.AuditLogs.GetAuditLogs;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.AuditLogs;

/// <summary>
/// Contains tests for get audit logs service.
/// </summary>
public class GetAuditLogsServiceTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task GetAsync_ShouldMapPagedAuditLogs_WhenLogsExist()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var pageResult = new PagedResult<AuditLog>
        {
            Page = 2,
            PageSize = 5,
            TotalCount = 7,
            Items = new List<AuditLog>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "LoginSuccess",
                    Module = "Auth",
                    EntityName = "User",
                    EntityId = userId.ToString(),
                    Details = "User logged in successfully.",
                    IpAddress = "127.0.0.1",
                    CreatedAt = createdAt
                }
            }
        };

        _auditLogRepositoryMock
            .Setup(x => x.SearchAsync(userId, "LoginSuccess", "Auth", null, null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageResult);

        var service = new GetAuditLogsService(_auditLogRepositoryMock.Object);
        var result = await service.GetAsync(userId, "LoginSuccess", "Auth", null, null, 2, 5);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(7);
        result.Items.Should().ContainSingle();
        result.Items[0].Action.Should().Be("LoginSuccess");
        result.Items[0].IpAddress.Should().Be("127.0.0.1");
    }
}
