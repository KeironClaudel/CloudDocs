using CloudDocs.Domain.Common;

namespace CloudDocs.Domain.Entities;

public class User : SoftDeletableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Department { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }

    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
}