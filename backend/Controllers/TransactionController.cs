using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Finance;

namespace FinanceTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public TransactionController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetTransactions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .Select(t => new TransactionViewDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Date = t.Date,
                Type = t.Type,
                CategoryName = t.Category.Name
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionViewDto>> GetTransactionById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transaction = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TransactionViewDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Date = t.Date,
                Type = t.Type,
                CategoryName = t.Category.Name
            })
            .FirstOrDefaultAsync();

        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        int categoryId = dto.CategoryId;
        
        if (categoryId == 0)
        {
            var uncategorized = await _db.Categories.FirstOrDefaultAsync(c =>
                c.UserId == userId && c.Name == "WITHOUT CATEGORY");

            if (uncategorized == null)
                return BadRequest("Default category not found");

            categoryId = uncategorized.Id;
        }

        var transaction = new Transaction
        {
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            CategoryId = categoryId,
            UserId = userId!
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null) return NotFound();

        transaction.Amount = dto.Amount;
        transaction.Date = dto.Date;
        transaction.Type = dto.Type;
        transaction.CategoryId = dto.CategoryId;
        await _db.SaveChangesAsync();

        return NoContent();
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
}