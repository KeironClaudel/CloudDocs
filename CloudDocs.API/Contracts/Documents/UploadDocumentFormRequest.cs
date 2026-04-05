using CloudDocs.Domain.Entities;
using CloudDocs.Domain.Enums;

namespace CloudDocs.API.Contracts.Documents;

/// <summary>
/// Represents the request data for upload document form.
/// </summary>
public class UploadDocumentFormRequest
{
    /// <summary>
    /// Gets or sets the file.
    /// </summary>
    public IFormFile File { get; set; } = null!;
    /// <summary>
    /// Gets or sets the category id.
    /// </summary>
    public Guid CategoryId { get; set; }
    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public Guid DocumentTypeId { get; set; }
    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
    /// <summary>
    /// Gets or sets the expiration date pending definition.
    /// </summary>
    public bool ExpirationDatePendingDefinition { get; set; }
    /// <summary>
    /// Gets or sets the access level.
    /// </summary>
    public Guid AccessLevelId { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public string? Department { get; set; }
}