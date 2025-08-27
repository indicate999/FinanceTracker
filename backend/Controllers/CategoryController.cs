using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using FinanceTracker.Utils;

namespace FinanceTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CategoryController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryViewDto>>> GetCategories(
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _db.Categories
            .Where(c => c.UserId == userId)
            .ProjectTo<CategoryViewDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();

        bool isAscending = sortOrder.ToLower() == "asc";
        query = (sortBy.ToLower()) switch
        {
            "type" => isAscending ? query.OrderBy(c => c.Type) : query.OrderByDescending(c => c.Type),
            _ => isAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
        };
        
        var categories = await query.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<ActionResult<IReadOnlyList<TransactionViewDto>>> GetTransactionsByCategory(
        int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var owns = await _db.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && c.UserId == userId);

        if (!owns) return NotFound();

        var pageSize = Math.Clamp(take, 1, 200);

        var dtos = await _db.Transactions
            .Where(t => t.UserId == userId && t.CategoryId == id)
            .OrderBy(t => t.Date)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ProjectTo<TransactionViewDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryViewDto>> GetCategoryById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .ProjectTo<CategoryViewDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        category.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound();

        if (category.Name == Constants.DefaultCategoryName)
            return BadRequest("This category cannot be edited.");

        var oldType = category.Type;
        
        _mapper.Map(dto, category);
        
        if (category.Type != oldType && category.Type != CategoryType.Neutral)
        {
            var defaultCategory = await _db.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == Constants.DefaultCategoryName);
            if (defaultCategory != null)
            {
                foreach (var transaction in category.Transactions.ToList())
                {
                    if ((category.Type == CategoryType.Income && transaction.Type == TransactionType.Expense) ||
                        (category.Type == CategoryType.Expense && transaction.Type == TransactionType.Income))
                    {
                        transaction.CategoryId = defaultCategory.Id;
                    }
                }
            }
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound();

        if (category.Name == Constants.DefaultCategoryName)
            return BadRequest("This category cannot be deleted.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}