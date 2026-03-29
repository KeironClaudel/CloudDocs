namespace CloudDocs.Application.Features.Categories.CreateCategory;

/// <summary>
/// Represents the request data for create category.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
public sealed record CreateCategoryRequest(
    string Name,
    string? Description);