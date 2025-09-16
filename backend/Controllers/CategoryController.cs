using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Services;

namespace FinanceTracker.Controllers;

/// <summary>
/// Manages user-defined categories for financial tracking.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves a list of categories for the authenticated user.
    /// </summary>
    /// <param name="sortBy">Field to sort by (e.g., "name", "type"). Defaults to "name".</param>
    /// <param name="sortOrder">Sort order ("asc" for ascending, "desc" for descending). Defaults to "asc".</param>
    /// <returns>A list of <see cref="CategoryViewDto"/> for the user.</returns>
    /// <response code="200">Returns the list of categories.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CategoryViewDto>>> GetCategories(
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var categories = await _categoryService.GetCategoriesAsync(userId, sortBy, sortOrder);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a paginated list of transactions associated with a specific category for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the category.</param>
    /// <param name="skip">The number of transactions to skip (for pagination).</param>
    /// <param name="take">The number of transactions to take (page size).</param>
    /// <returns>A paginated list of <see cref="TransactionViewDto"/>.</returns>
    /// <response code="200">Returns the list of transactions.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the category is not found or does not belong to the user.</response>
    [HttpGet("{id:int}/transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TransactionViewDto>>> GetTransactionsByCategory(
        int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var dtos = await _categoryService.GetTransactionsByCategoryAsync(id, userId, skip, take);
        if (dtos == null) return NotFound(); // If category not found or unauthorized
        
        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves a single category by its ID for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the category to retrieve.</param>
    /// <returns>The <see cref="CategoryViewDto"/> if found.</returns>
    /// <response code="200">Returns the requested category.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the category is not found or does not belong to the user.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryViewDto>> GetCategoryById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var category = await _categoryService.GetCategoryByIdAsync(id, userId);
        if (category == null) return NotFound();
        return Ok(category);
    }

    /// <summary>
    /// Creates a new category for the authenticated user.
    /// </summary>
    /// <param name="dto">The category details.</param>
    /// <returns>The newly created <see cref="CategoryViewDto"/>.</returns>
    /// <response code="200">Returns the created category.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If the input data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(CategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var createdCategory = await _categoryService.CreateCategoryAsync(userId, dto);
        return Ok(createdCategory);
    }

    /// <summary>
    /// Updates an existing category for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the category to update.</param>
    /// <param name="dto">The updated category details.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the category is successfully updated.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If the input data is invalid or the category cannot be edited.</response>
    /// <response code="404">If the category is not found or does not belong to the user.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, error) = await _categoryService.UpdateCategoryAsync(id, userId, dto);
        if (!success)
        {
            if (error == "Category not found.") return NotFound(error);
            return BadRequest(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Deletes a category for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <returns>No content if deletion is successful.</returns>
    /// <response code="204">If the category is successfully deleted.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If the category cannot be deleted (e.g., default category).</response>
    /// <response code="404">If the category is not found or does not belong to the user.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, error) = await _categoryService.DeleteCategoryAsync(id, userId);
        if (!success)
        {
            if (error == "Category not found.") return NotFound(error);
            return BadRequest(error);
        }
        return NoContent();
    }
}