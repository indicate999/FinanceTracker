using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using System.Security.Claims;
using AutoMapper;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using FinanceTracker.Utils;

namespace FinanceTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public TransactionController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetTransactions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var result = _mapper.Map<List<TransactionViewDto>>(transactions);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionViewDto>> GetTransactionById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transaction = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.Id == id && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null) return NotFound();
        
        var result = _mapper.Map<TransactionViewDto>(transaction);
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        int categoryId = dto.CategoryId;
        
        if (categoryId == 0)
        {
            var uncategorized = await _db.Categories.FirstOrDefaultAsync(c =>
                c.UserId == userId && c.Name == Constants.DefaultCategoryName);

            if (uncategorized == null)
                return BadRequest("Default category not found");

            categoryId = uncategorized.Id;
        }
        
        var category = await _db.Categories.FirstOrDefaultAsync(c =>
            c.Id == categoryId && c.UserId == userId);

        if (category == null)
            return BadRequest("Category not found");

        if (!IsCompatible(dto.Type, category.Type))
            return BadRequest("Category type does not match transaction type");

        var transaction = _mapper.Map<Transaction>(dto);
        transaction.CategoryId = categoryId;
        transaction.UserId = userId!;
        transaction.Date = transaction.Date.ToUniversalTime();

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        
        var result = _mapper.Map<TransactionViewDto>(transaction);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId == null)
            return Unauthorized();
        
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null) return NotFound();
        
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId);

        if (category == null)
            return BadRequest("Category not found");

        if (!IsCompatible(dto.Type, category.Type))
            return BadRequest("Category type does not match transaction type");
        
        _mapper.Map(dto, transaction);
        transaction.Date = transaction.Date.ToUniversalTime();
        
        await _db.SaveChangesAsync();

        var result = _mapper.Map<TransactionViewDto>(transaction);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null) return NotFound();

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    private bool IsCompatible(TransactionType transactionType, CategoryType categoryType)
    {
        return categoryType == CategoryType.Neutral
               || (transactionType == TransactionType.Income && categoryType == CategoryType.Income)
               || (transactionType == TransactionType.Expense && categoryType == CategoryType.Expense);
    }
}