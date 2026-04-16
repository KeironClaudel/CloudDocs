namespace CloudDocs.Domain.Entities;

/// <summary>
/// Represents an email sent by the system.
/// </summary>
public class SentEmailLog
{
    public Guid Id { get; set; }

    public Guid SentByUserId { get; set; }
    public User SentByUser { get; set; } = null!;

    public Guid? DocumentId { get; set; }
    public Document? Document { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}