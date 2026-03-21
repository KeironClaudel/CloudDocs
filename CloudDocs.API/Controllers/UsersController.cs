using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Application.Features.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IGetUsersService _getUsersService;
    private readonly IGetUserByIdService _getUserByIdService;
    private readonly ICreateUserService _createUserService;
    private readonly IUpdateUserService _updateUserService;
    private readonly IDeactivateUserService _deactivateUserService;
    private readonly IReactivateUserService _reactivateUserService;

    public UsersController(
        IGetUsersService getUsersService,
        IGetUserByIdService getUserByIdService,
        ICreateUserService createUserService,
        IUpdateUserService updateUserService,
        IDeactivateUserService deactivateUserService,
        IReactivateUserService reactivateUserService)
    {
        _getUsersService = getUsersService;
        _getUserByIdService = getUserByIdService;
        _createUserService = createUserService;
        _updateUserService = updateUserService;
        _deactivateUserService = deactivateUserService;
        _reactivateUserService = reactivateUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _getUsersService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _getUserByIdService.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var createdUser = await _createUserService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updatedUser = await _updateUserService.UpdateAsync(id, request, cancellationToken);
        if (updatedUser is null)
            return NotFound(new { message = "User not found." });
        return Ok(updatedUser);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateUserService.DeactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "User not found." });
        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateUserService.ReactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "User not found." });
        return NoContent();
    }
}