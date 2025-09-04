using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetTransactions(
        [FromQuery] string sortBy = "date", 
        [FromQuery] string sortOrder = "desc")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var transactions = await _transactionService.GetTransactionsAsync(userId, sortBy, sortOrder);
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionViewDto>> GetTransactionById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (createdTransaction, error) = await _transactionService.CreateTransactionAsync(userId, dto);
        if (createdTransaction == null)
            return BadRequest(error);
        
        return Ok(createdTransaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (updatedDto, error) = await _transactionService.UpdateTransactionAsync(id, userId, dto);
        if (updatedDto == null)
        {
            if (error == "Transaction not found or unauthorized.") return NotFound(error);
            if (error == "Category not found or unauthorized.") return BadRequest(error);
            if (error == "Category type does not match transaction type.") return BadRequest(error);
            return BadRequest(error); // Generic bad request for other errors
        }
        
        return Ok(updatedDto); // Return the updated DTO to the frontend
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (success, error) = await _transactionService.DeleteTransactionAsync(id, userId);
        if (!success)
        {
            if (error == "Transaction not found or unauthorized.") return NotFound(error);
            return BadRequest(error);
        }
        return NoContent();
    }
}