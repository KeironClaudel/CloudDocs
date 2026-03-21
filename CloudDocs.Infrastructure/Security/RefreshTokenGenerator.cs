using System.Security.Cryptography;
using CloudDocs.Application.Common.Interfaces.Security;

namespace CloudDocs.Infrastructure.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}