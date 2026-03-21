namespace CloudDocs.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}