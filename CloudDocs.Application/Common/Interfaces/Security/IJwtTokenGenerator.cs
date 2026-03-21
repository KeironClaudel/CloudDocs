namespace CloudDocs.Application.Common.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role);
}