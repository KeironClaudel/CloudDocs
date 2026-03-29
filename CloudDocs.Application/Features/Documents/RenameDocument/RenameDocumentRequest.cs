namespace CloudDocs.Application.Features.Documents.RenameDocument;

/// <summary>
/// Represents the request data for rename document.
/// </summary>
/// <param name="NewName">The new name.</param>
public sealed record RenameDocumentRequest(string NewName);