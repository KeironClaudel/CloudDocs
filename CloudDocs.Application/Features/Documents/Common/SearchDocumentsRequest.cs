using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.Common;

public sealed record SearchDocumentsRequest(
    string? Name,
    Guid? CategoryId,
    int? Month,
    int? Year,
    DocumentType? DocumentType,
    bool? ExpirationPendingDefinition,
    int Page = 1,
    int PageSize = 10);