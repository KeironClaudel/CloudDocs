using CloudDocs.Domain.Enums;

namespace CloudDocs.API.Contracts.Documents;

public class UploadDocumentFormRequest
{
    public IFormFile File { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public DocumentType DocumentType { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool ExpirationDatePendingDefinition { get; set; }
    public DocumentAccessLevel AccessLevel { get; set; }
    public string? Department { get; set; }
}