using System.Security.Claims;
using CloudDocs.Application.Features.Auth.ChangePassword;
using CloudDocs.Application.Features.Auth.ForgotPassword;
using CloudDocs.Application.Features.Auth.Login;
using CloudDocs.Application.Features.Auth.ResetPassword;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CloudDocs.Application.Features.Auth.Logout;
using CloudDocs.Application.Features.Auth.RefreshToken;

namespace CloudDocs.API.Controllers;

/// <summary>
/// Exposes endpoints for auth.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IForgotPasswordService _forgotPasswordService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly IChangePasswordService _changePasswordService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogoutService _logoutService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="loginService">The login service.</param>
    /// <param name="forgotPasswordService">The forgot password service.</param>
    /// <param name="resetPasswordService">The reset password service.</param>
    /// <param name="changePasswordService">The change password service.</param>
    /// <param name="refreshTokenService">The refresh token service.</param>
    /// <param name="logoutService">The logout service.</param>
    public AuthController(
    ILoginService loginService,
    IForgotPasswordService forgotPasswordService,
    IResetPasswordService resetPasswordService,
    IChangePasswordService changePasswordService,
    IRefreshTokenService refreshTokenService,
    ILogoutService logoutService)
    {
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
        _resetPasswordService = resetPasswordService;
        _changePasswordService = changePasswordService;
        _refreshTokenService = refreshTokenService;
        _logoutService = logoutService;
    }

    /// <summary>
    /// Validates user credentials and if true, returns a JWT token with user information
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _loginService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Processes password when the user doesn't remember it.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await _forgotPasswordService.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Resets the password.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _resetPasswordService.ExecuteAsync(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Changes the password.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        await _changePasswordService.ExecuteAsync(userId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Refreshes the token.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _refreshTokenService.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _logoutService.ExecuteAsync(request, cancellationToken);
        return NoContent();
    }
}