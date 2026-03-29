using CloudDocs.Application.Features.Categories.CreateCategory;
using CloudDocs.Application.Features.Categories.DeactivateCategory;
using CloudDocs.Application.Features.Categories.GetCategories;
using CloudDocs.Application.Features.Categories.GetCategoryById;
using CloudDocs.Application.Features.Categories.UpdateCategory;
using CloudDocs.Application.Features.Categories.ReactivateCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

/// <summary>
/// Exposes endpoints for categories.
/// </summary>
[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IGetCategoriesService _getCategoriesService;
    private readonly IGetCategoryByIdService _getCategoryByIdService;
    private readonly ICreateCategoryService _createCategoryService;
    private readonly IUpdateCategoryService _updateCategoryService;
    private readonly IDeactivateCategoryService _deactivateCategoryService;
    private readonly IReactivateCategoryService _reactivateCategoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/> class.
    /// </summary>
    /// <param name="getCategoriesService">The get categories service.</param>
    /// <param name="getCategoryByIdService">The get category by id service.</param>
    /// <param name="createCategoryService">The create category service.</param>
    /// <param name="updateCategoryService">The update category service.</param>
    /// <param name="deactivateCategoryService">The deactivate category service.</param>
    /// <param name="reactivateCategoryService">The reactivate category service.</param>
    public CategoriesController(
        IGetCategoriesService getCategoriesService,
        IGetCategoryByIdService getCategoryByIdService,
        ICreateCategoryService createCategoryService,
        IUpdateCategoryService updateCategoryService,
        IDeactivateCategoryService deactivateCategoryService,
        IReactivateCategoryService reactivateCategoryService    )
    {
        _getCategoriesService = getCategoriesService;
        _getCategoryByIdService = getCategoryByIdService;
        _createCategoryService = createCategoryService;
        _updateCategoryService = updateCategoryService;
        _deactivateCategoryService = deactivateCategoryService;
        _reactivateCategoryService = reactivateCategoryService;
    }

    /// <summary>
    /// Gets all items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _getCategoriesService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _getCategoryByIdService.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    /// <summary>
    /// Creates.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var created = await _createCategoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var updated = await _updateCategoryService.UpdateAsync(id, request, cancellationToken);

        if (updated is null)
            return NotFound(new { message = "Category not found." });

        return Ok(updated);
    }

    /// <summary>
    /// Deactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateCategoryService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Category not found." });

        return NoContent();
    }

    /// <summary>
    /// Reactivates.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the action result.</returns>
    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _reactivateCategoryService.ReactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Category not found." });

        return NoContent();
    }
}