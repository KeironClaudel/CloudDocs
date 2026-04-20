using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Features.Clients.CreateClient;
using CloudDocs.Application.Features.Clients.DeactivateClient;
using CloudDocs.Application.Features.Clients.GetClientById;
using CloudDocs.Application.Features.Clients.GetClients;
using CloudDocs.Application.Features.Clients.ReactivateClient;
using CloudDocs.Application.Features.Clients.SearchClients;
using CloudDocs.Application.Features.Clients.UpdateClient;
using CloudDocs.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CloudDocs.Tests.Services.Clients;

/// <summary>
/// Contains tests for client services without existing coverage.
/// </summary>
public class ClientServicesTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private CreateClientService CreateCreateService() =>
        new(_clientRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object);

    private UpdateClientService CreateUpdateService() =>
        new(_clientRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object);

    private DeactivateClientService CreateDeactivateService() =>
        new(_clientRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object);

    private ReactivateClientService CreateReactivateService() =>
        new(_clientRepositoryMock.Object, _auditServiceMock.Object, _unitOfWorkMock.Object);

    private GetClientsService CreateGetAllService() =>
        new(_clientRepositoryMock.Object);

    private SearchClientsService CreateSearchService() =>
        new(_clientRepositoryMock.Object);

    private GetClientByIdService CreateGetByIdService() =>
        new(_clientRepositoryMock.Object);

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequest_WhenNameAlreadyExists()
    {
        var request = new CreateClientRequest("Contoso", null, null, null, null, null);

        _clientRepositoryMock
            .Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await CreateCreateService().CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Client name is already in use.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateClient_WhenRequestIsValid()
    {
        var request = new CreateClientRequest("  Contoso  ", " Legal ", " ID-1 ", " test@contoso.com ", " 123 ", " note ");

        _clientRepositoryMock
            .Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _clientRepositoryMock
            .Setup(x => x.IdentificationExistsAsync(" ID-1 ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateCreateService().CreateAsync(request);

        result.Name.Should().Be("Contoso");
        result.LegalName.Should().Be("Legal");
        result.Identification.Should().Be("ID-1");
        result.Email.Should().Be("test@contoso.com");
        result.Phone.Should().Be("123");
        result.Notes.Should().Be("note");
        result.IsActive.Should().BeTrue();

        _clientRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Client>(c =>
                c.Name == "Contoso" &&
                c.LegalName == "Legal" &&
                c.Identification == "ID-1"), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest("Contoso", null, null, null, null, null);

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var result = await CreateUpdateService().UpdateAsync(clientId, request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequest_WhenIdentificationBelongsToAnotherClient()
    {
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest("Contoso", null, "ID-2", null, null, null);
        var entity = new Client { Id = clientId, Name = "Contoso", Identification = "ID-1", IsActive = true };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _clientRepositoryMock.Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _clientRepositoryMock.Setup(x => x.IdentificationExistsAsync(request.Identification!, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await CreateUpdateService().UpdateAsync(clientId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Client identification is already in use.");
    }

    [Fact]
    public async Task DeactivateAsync_ShouldReturnFalse_WhenClientDoesNotExist()
    {
        var clientId = Guid.NewGuid();
        _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        var result = await CreateDeactivateService().DeactivateAsync(clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateClient_WhenClientExists()
    {
        var clientId = Guid.NewGuid();
        var entity = new Client
        {
            Id = clientId,
            Name = "Contoso",
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await CreateReactivateService().ReactivateAsync(clientId);

        result.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.DeletedAt.Should().BeNull();
        _clientRepositoryMock.Verify(x => x.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldMapClients_WhenClientsExist()
    {
        var createdAt = DateTime.UtcNow;
        var updatedAt = createdAt.AddHours(2);
        _clientRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Client>
        {
            new() { Id = Guid.NewGuid(), Name = "Contoso", LegalName = "Contoso Ltd", Identification = "ID-001", IsActive = true, CreatedAt = createdAt, UpdatedAt = updatedAt }
        });

        var result = await CreateGetAllService().GetAllAsync();

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Contoso");
        result[0].LegalName.Should().Be("Contoso Ltd");
        result[0].DisplayName.Should().Be("Contoso - ID-001");
        result[0].UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnDisplayName_WithIdentification_WhenAvailable()
    {
        _clientRepositoryMock
            .Setup(x => x.SearchByNameAsync("Cont", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Keiron Claudel",
                    Identification = "123456789",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            });

        var result = await CreateSearchService().SearchByNameAsync("Cont");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Keiron Claudel");
        result[0].Identification.Should().Be("123456789");
        result[0].DisplayName.Should().Be("Keiron Claudel - 123456789");
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnEmptyList_WhenTermIsBlank()
    {
        var result = await CreateSearchService().SearchByNameAsync("   ");
        result.Should().BeEmpty();
        _clientRepositoryMock.Verify(x => x.SearchByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        var clientId = Guid.NewGuid();
        _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        var result = await CreateGetByIdService().GetByIdAsync(clientId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedAt_WhenClientIsUpdated()
    {
        var clientId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-2);
        var entity = new Client
        {
            Id = clientId,
            Name = "Contoso",
            Identification = "ID-1",
            IsActive = true,
            CreatedAt = createdAt
        };

        var request = new UpdateClientRequest("Contoso Updated", "Legal", "ID-1", "mail@test.com", "123", "note");

        _clientRepositoryMock
            .Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _clientRepositoryMock
            .Setup(x => x.NameExistsAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _clientRepositoryMock
            .Setup(x => x.IdentificationExistsAsync(request.Identification!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateUpdateService().UpdateAsync(clientId, request);

        result.Should().NotBeNull();
        result!.UpdatedAt.Should().NotBeNull();
        result.CreatedAt.Should().Be(createdAt);
        entity.UpdatedAt.Should().NotBeNull();
    }
}
