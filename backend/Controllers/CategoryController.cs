using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.Models.Finance;

namespace FinanceTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryViewDto>>> GetCategories()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var categories = await _db.Categories
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryViewDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryViewDto>> GetCategoryById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryViewDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .FirstOrDefaultAsync();

        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Type = dto.Type,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound();

        category.Name = dto.Name;
        category.Type = dto.Type;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}