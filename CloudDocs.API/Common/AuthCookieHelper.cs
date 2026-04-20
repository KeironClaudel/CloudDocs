using CloudDocs.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CloudDocs.API.Common;

public class AuthCookieHelper
{
    private readonly AuthCookieSettings _settings;
    private readonly JwtSettings _jwtSettings;

    public AuthCookieHelper(
        IOptions<AuthCookieSettings> options,
        IOptions<JwtSettings> jwtOptions)
    {
        _settings = options.Value;
        _jwtSettings = jwtOptions.Value;
    }

    public void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
    {
        response.Cookies.Append(
            _settings.AccessTokenCookieName,
            accessToken,
            BuildAccessTokenOptions());

        response.Cookies.Append(
            _settings.RefreshTokenCookieName,
            refreshToken,
            BuildRefreshTokenOptions());
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(
            _settings.AccessTokenCookieName,
            BuildAccessTokenOptions());

        response.Cookies.Delete(
            _settings.RefreshTokenCookieName,
            BuildRefreshTokenOptions());
    }

    public string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[_settings.RefreshTokenCookieName];
    }

    private CookieOptions BuildAccessTokenOptions()
    {
        var accessTokenLifetime = TimeSpan.FromMinutes(
            _jwtSettings.ExpirationMinutes > 0 ? _jwtSettings.ExpirationMinutes : 15);

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Expires = DateTimeOffset.UtcNow.Add(accessTokenLifetime),
            MaxAge = accessTokenLifetime,
            IsEssential = true,
            Path = "/"
        };
    }

    private CookieOptions BuildRefreshTokenOptions()
    {
        var refreshTokenLifetime = TimeSpan.FromDays(
            _settings.RefreshTokenDays > 0 ? _settings.RefreshTokenDays : 7);

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Expires = DateTimeOffset.UtcNow.Add(refreshTokenLifetime),
            MaxAge = refreshTokenLifetime,
            IsEssential = true,
            Path = "/api/auth"
        };
    }

    private static SameSiteMode ParseSameSite(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }
}
