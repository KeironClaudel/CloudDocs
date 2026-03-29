namespace CloudDocs.Application.Features.Categories.Common;

/// <summary>
/// Represents the response data for category.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="IsActive">The is active.</param>
/// <param name="CreatedAt">The created at.</param>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
    );