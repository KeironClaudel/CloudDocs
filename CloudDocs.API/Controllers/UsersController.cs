using CloudDocs.Application.Features.Users.CreateUser;
using CloudDocs.Application.Features.Users.DeactivateUser;
using CloudDocs.Application.Features.Users.GetUserById;
using CloudDocs.Application.Features.Users.GetUsers;
using CloudDocs.Application.Features.Users.ReactivateUser;
using CloudDocs.Application.Features.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

/// <summary>
/// Exposes endpoints for users.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="getUsersService">The get users service.</param>
    /// <param name="getUserByIdService">The get user by id service.</param>
    /// <param name="createUserService">The create user service.</param>
    /// <param name="updateUserService">The update user service.</param>
    /// <param name="deactivateUserService">The deactivate user service.</param>
    /// <param name="reactivateUserService">The reactivate user service.</param>
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

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _getUsersService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _getUserByIdService.GetByIdAsync(id, cancellationToken);

        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }

    /// <summary>
    /// Creates.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var createdUser = await _createUserService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updatedUser = await _updateUserService.UpdateAsync(id, request, cancellationToken);
        if (updatedUser is null)
            return NotFound(new { message = "User not found." });
        return Ok(updatedUser);
    }

    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateUserService.DeactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "User not found." });
        return NoContent();
    }

    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateUserService.ReactivateAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "User not found." });
        return NoContent();
    }
}