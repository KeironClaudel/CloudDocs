namespace CloudDocs.Application.Features.Categories.UpdateCategory;

/// <summary>
/// Represents the request data for update category.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
public sealed record UpdateCategoryRequest(
    string Name,
    string? Description);