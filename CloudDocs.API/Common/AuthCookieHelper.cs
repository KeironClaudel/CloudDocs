using CloudDocs.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CloudDocs.API.Common;

public class AuthCookieHelper
{
    private readonly AuthCookieSettings _settings;

    public AuthCookieHelper(IOptions<AuthCookieSettings> options)
    {
        _settings = options.Value;
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
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Expires = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            IsEssential = true,
            Path = "/"
        };
    }

    private CookieOptions BuildRefreshTokenOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = ParseSameSite(_settings.SameSite),
            Expires = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),
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