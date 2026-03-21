namespace CloudDocs.API.Contracts.Documents;

public class UploadDocumentVersionFormRequest
{
    public IFormFile File { get; set; } = null!;
}