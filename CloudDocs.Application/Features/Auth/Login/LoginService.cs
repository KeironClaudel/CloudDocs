using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = CloudDocs.Domain.Entities.RefreshToken;

namespace CloudDocs.Application.Features.Auth.Login;

/// <summary>
/// Provides operations for login.
/// </summary>
public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuditService _auditService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginService> _logger;
    private readonly AuthCookieSettings _authCookieSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="jwtTokenGenerator">The jwt token generator.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="refreshTokenRepository">The refresh token repository.</param>
    /// <param name="refreshTokenGenerator">The refresh token generator.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public LoginService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IAuditService auditService,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    IOptions<AuthCookieSettings> authCookieOptions,
    ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _auditService = auditService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
        _authCookieSettings = authCookieOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user using email and password, enforcing security checks such as
    /// account status, lockout policies, and failed login attempts.
    /// Generates a JWT access token and a refresh token upon successful authentication,
    /// and records all login attempts through audit logging.
    /// </summary>
    /// <param name="request">The login request containing user credentials.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="LoginResponse"/> containing the access token, refresh token,
    /// and user information upon successful authentication.
    /// </returns>
    /// <exception cref="UnauthorizedException">
    /// Thrown when authentication fails due to invalid credentials, inactive user,
    /// or account lockout.
    /// </exception>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            await _auditService.LogAsync(
                null,
                "LoginFailed",
                "Auth",
                "User",
                null,
                $"Failed login attempt for email: {request.Email}",
                null,
                cancellationToken);

            _logger.LogWarning("Login failed. User with email {Email} was not found.", request.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            await _auditService.LogAsync(
                user.Id,
                "LoginFailed",
                "Auth",
                "User",
                user.Id.ToString(),
                "Inactive user attempted login.",
                null,
                cancellationToken);

            throw new UnauthorizedException("User is inactive.");
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc > DateTime.UtcNow)
        {
            await _auditService.LogAsync(
                user.Id,
                "LoginBlocked",
                "Auth",
                "User",
                user.Id.ToString(),
                "Locked user attempted login.",
                null,
                cancellationToken);

            throw new UnauthorizedException("User is temporarily locked.");
        }

        var isValidPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
                user.Id,
                "LoginFailed",
                "Auth",
                "User",
                user.Id.ToString(),
                "Invalid password.",
                null,
                cancellationToken);

            _logger.LogWarning("Login failed for user {Email}. Invalid password.", user.Email);
            throw new UnauthorizedException("Invalid credentials.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastActivityAtUtc = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
                                    user.Id,
                                    "LoginSuccess",
                                    "Auth",
                                    "User",
                                    user.Id.ToString(),
                                    "User logged in successfully.",
                                    null,
                                    cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.Name);

        var refreshTokenValue = _refreshTokenGenerator.Generate();

        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _authCookieSettings.RefreshTokenDays > 0 ? _authCookieSettings.RefreshTokenDays : 7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);
        return new LoginResponse(
    token,
    refreshTokenValue,
    user.FullName,
    user.Email,
    user.Role.Name);
    }
}
