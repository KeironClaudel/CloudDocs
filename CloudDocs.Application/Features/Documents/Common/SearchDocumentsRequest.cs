using CloudDocs.Domain.Enums;

namespace CloudDocs.Application.Features.Documents.Common;

/// <summary>
/// Represents the request data for search documents.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="CategoryId">The category id identifier.</param>
/// <param name="Month">The month.</param>
/// <param name="Year">The year.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="ExpirationPendingDefinition">The expiration pending definition.</param>
/// <param name="IncludeInactive">The include inactive.</param>
/// <param name="Page">The page number.</param>
/// <param name="PageSize">The page size.</param>
public sealed record SearchDocumentsRequest(
    string? Name,
    Guid? CategoryId,
    int? Month,
    int? Year,
    DocumentType? DocumentType,
    bool? ExpirationPendingDefinition,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 10);