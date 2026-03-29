namespace CloudDocs.API.Contracts.Documents;

/// <summary>
/// Represents the request data for upload document version form.
/// </summary>
public class UploadDocumentVersionFormRequest
{
    /// <summary>
    /// Gets or sets the file.
    /// </summary>
    public IFormFile File { get; set; } = null!;
}