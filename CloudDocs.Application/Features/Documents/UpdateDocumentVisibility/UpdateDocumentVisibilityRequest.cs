namespace CloudDocs.Application.Features.Documents.UpdateDocumentVisibility;

public sealed record UpdateDocumentVisibilityRequest(
    Guid AccessLevelId,
    List<Guid>? DepartmentIds);