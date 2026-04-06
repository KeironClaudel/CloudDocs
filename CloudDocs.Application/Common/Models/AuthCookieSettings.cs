namespace CloudDocs.Application.Common.Models;

public class AuthCookieSettings
{
    public const string SectionName = "AuthCookies";

    public string AccessTokenCookieName { get; set; } = "clouddocs_access_token";
    public string RefreshTokenCookieName { get; set; } = "clouddocs_refresh_token";

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Lax"; // Lax | Strict | None
}
