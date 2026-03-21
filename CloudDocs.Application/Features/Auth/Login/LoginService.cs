using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Security;
using CloudDocs.Application.Common.Interfaces.Services;
using RefreshTokenEntity = CloudDocs.Domain.Entities.RefreshToken;
using CloudDocs.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace CloudDocs.Application.Features.Auth.Login;

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

    public LoginService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IAuditService auditService,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    ILogger<LoginService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _auditService = auditService;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

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
            ExpiresAt = DateTime.UtcNow.AddDays(7),
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