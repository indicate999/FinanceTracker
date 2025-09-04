using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Services;

namespace FinanceTracker.Controllers;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryViewDto>>> GetCategories(
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var categories = await _categoryService.GetCategoriesAsync(userId, sortBy, sortOrder);
        return Ok(categories);
    }

    [HttpGet("{id:int}/transactions")]
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

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryViewDto>> GetCategoryById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var category = await _categoryService.GetCategoryByIdAsync(id, userId);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var createdCategory = await _categoryService.CreateCategoryAsync(userId, dto);
        return Ok(createdCategory);
    }

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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