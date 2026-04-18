using CloudDocs.API.Common;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Features.Auth.ChangePassword;
using CloudDocs.Application.Features.Auth.ForgotPassword;
using CloudDocs.Application.Features.Auth.Login;
using CloudDocs.Application.Features.Auth.Logout;
using CloudDocs.Application.Features.Auth.Me;
using CloudDocs.Application.Features.Auth.RefreshToken;
using CloudDocs.Application.Features.Auth.ResetPassword;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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
    private readonly AuthCookieHelper _authCookieHelper;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="loginService">The login service.</param>
    /// <param name="forgotPasswordService">The forgot password service.</param>
    /// <param name="resetPasswordService">The reset password service.</param>
    /// <param name="changePasswordService">The change password service.</param>
    /// <param name="refreshTokenService">The refresh token service.</param>
    /// <param name="authCookieHelper">The Cookie helper.</param>
    /// <param name="userRepository"> The userRepository interface</param>
    /// <param name="logoutService">The logout service.</param>
    public AuthController(
    ILoginService loginService,
    IForgotPasswordService forgotPasswordService,
    IResetPasswordService resetPasswordService,
    IChangePasswordService changePasswordService,
    IRefreshTokenService refreshTokenService,
    AuthCookieHelper authCookieHelper,
    IUserRepository userRepository,
    ILogoutService logoutService)
    {
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
        _resetPasswordService = resetPasswordService;
        _changePasswordService = changePasswordService;
        _refreshTokenService = refreshTokenService;
        _authCookieHelper = authCookieHelper;
        _userRepository = userRepository;
        _logoutService = logoutService;
    }

    /// <summary>
    /// Retrieves information about the currently authenticated user.
    /// </summary>
    /// <remarks>This endpoint requires authentication. The response includes user information such as ID,
    /// full name, email, and role. If the user's authentication token is invalid or the user is inactive, the request
    /// is denied.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>An HTTP 200 response containing the current user's details if authentication and user status checks succeed;
    /// otherwise, an HTTP 401 response if the user is not authenticated or is inactive.</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user token." });

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "User not found or inactive." });

        var response = new CurrentUserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.Name);

        return Ok(response);
    }

    /// <summary>
    /// Validates user credentials and if true, returns a JWT token with user information
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _loginService.LoginAsync(request, cancellationToken);

        _authCookieHelper.SetAuthCookies(Response, result.AccessToken, result.RefreshToken);

        var clientResponse = new LoginClientResponse(
            result.FullName,
            result.Email,
            result.Role);

        return Ok(clientResponse);
    }

    /// <summary>
    /// Processes password when the user doesn't remember it.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimitPolicyNames.AuthStrict)]
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
    /// Refreshes the auth token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = _authCookieHelper.GetRefreshToken(Request);

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        var result = await _refreshTokenService.ExecuteAsync(
            new RefreshTokenRequest(refreshToken),
            cancellationToken);

        _authCookieHelper.SetAuthCookies(Response, result.AccessToken, result.RefreshToken);

        return NoContent();
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = _authCookieHelper.GetRefreshToken(Request);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _logoutService.ExecuteAsync(new LogoutRequest(refreshToken), cancellationToken);
        }

        _authCookieHelper.ClearAuthCookies(Response);

        return NoContent();
    }
}
