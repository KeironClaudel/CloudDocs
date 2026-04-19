using CloudDocs.Application.Features.Clients.CreateClient;
using CloudDocs.Application.Features.Clients.DeactivateClient;
using CloudDocs.Application.Features.Clients.GetClientById;
using CloudDocs.Application.Features.Clients.GetClients;
using CloudDocs.Application.Features.Clients.ReactivateClient;
using CloudDocs.Application.Features.Clients.SearchClients;
using CloudDocs.Application.Features.Clients.UpdateClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IGetClientsService _getClientsService;
    private readonly IGetClientByIdService _getClientByIdService;
    private readonly ISearchClientsService _searchClientsService;
    private readonly ICreateClientService _createClientService;
    private readonly IUpdateClientService _updateClientService;
    private readonly IDeactivateClientService _deactivateClientService;
    private readonly IReactivateClientService _reactivateClientService;

    public ClientsController(
        IGetClientsService getClientsService,
        IGetClientByIdService getClientByIdService,
        ISearchClientsService searchClientsService,
        ICreateClientService createClientService,
        IUpdateClientService updateClientService,
        IDeactivateClientService deactivateClientService,
        IReactivateClientService reactivateClientService)
    {
        _getClientsService = getClientsService;
        _getClientByIdService = getClientByIdService;
        _searchClientsService = searchClientsService;
        _createClientService = createClientService;
        _updateClientService = updateClientService;
        _deactivateClientService = deactivateClientService;
        _reactivateClientService = reactivateClientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getClientsService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByName([FromQuery] string term, CancellationToken cancellationToken)
    {
        var result = await _searchClientsService.SearchByNameAsync(term, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getClientByIdService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Client not found." });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var result = await _createClientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var result = await _updateClientService.UpdateAsync(id, request, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Client not found." });

        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateClientService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Client not found." });

        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateClientService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Client not found." });

        return NoContent();
    }
}