using CloudDocs.Application.Features.Categories.CreateCategory;
using CloudDocs.Application.Features.Categories.DeactivateCategory;
using CloudDocs.Application.Features.Categories.GetCategories;
using CloudDocs.Application.Features.Categories.GetCategoryById;
using CloudDocs.Application.Features.Categories.UpdateCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudDocs.API.Controllers;

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

    public CategoriesController(
        IGetCategoriesService getCategoriesService,
        IGetCategoryByIdService getCategoryByIdService,
        ICreateCategoryService createCategoryService,
        IUpdateCategoryService updateCategoryService,
        IDeactivateCategoryService deactivateCategoryService)
    {
        _getCategoriesService = getCategoriesService;
        _getCategoryByIdService = getCategoryByIdService;
        _createCategoryService = createCategoryService;
        _updateCategoryService = updateCategoryService;
        _deactivateCategoryService = deactivateCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _getCategoriesService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _getCategoryByIdService.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var created = await _createCategoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var updated = await _updateCategoryService.UpdateAsync(id, request, cancellationToken);

        if (updated is null)
            return NotFound(new { message = "Category not found." });

        return Ok(updated);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deactivateCategoryService.DeactivateAsync(id, cancellationToken);

        if (!success)
            return NotFound(new { message = "Category not found." });

        return NoContent();
    }
}