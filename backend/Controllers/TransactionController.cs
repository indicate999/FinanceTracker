using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Services;

namespace FinanceTracker.Controllers;

/// <summary>
/// Manages financial transactions for the authenticated user.
/// </summary>
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

    /// <summary>
    /// Retrieves a list of transactions for the authenticated user.
    /// </summary>
    /// <param name="sortBy">Field to sort by (e.g., "date", "amount", "type", "category"). Defaults to "date".</param>
    /// <param name="sortOrder">Sort order ("asc" for ascending, "desc" for descending). Defaults to "desc".</param>
    /// <returns>A list of <see cref="TransactionViewDto"/> for the user.</returns>
    /// <response code="200">Returns the list of transactions.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TransactionViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TransactionViewDto>>> GetTransactions(
        [FromQuery] string sortBy = "date", 
        [FromQuery] string sortOrder = "desc")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var transactions = await _transactionService.GetTransactionsAsync(userId, sortBy, sortOrder);
        return Ok(transactions);
    }

    /// <summary>
    /// Retrieves a single transaction by its ID for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the transaction to retrieve.</param>
    /// <returns>The <see cref="TransactionViewDto"/> if found.</returns>
    /// <response code="200">Returns the requested transaction.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the transaction is not found or does not belong to the user.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionViewDto>> GetTransactionById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    /// <summary>
    /// Creates a new transaction for the authenticated user.
    /// </summary>
    /// <param name="dto">The transaction details.</param>
    /// <returns>The newly created <see cref="TransactionViewDto"/>.</returns>
    /// <response code="200">Returns the created transaction.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If the input data is invalid, category not found, or category type mismatch.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction(TransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var (createdTransaction, error) = await _transactionService.CreateTransactionAsync(userId, dto);
        if (createdTransaction == null)
            return BadRequest(error);
        
        return Ok(createdTransaction);
    }

    /// <summary>
    /// Updates an existing transaction for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the transaction to update.</param>
    /// <param name="dto">The updated transaction details.</param>
    /// <returns>The updated <see cref="TransactionViewDto"/>.</returns>
    /// <response code="200">Returns the updated transaction.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If the input data is invalid, category not found, or category type mismatch.</response>
    /// <response code="404">If the transaction is not found or does not belong to the user.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TransactionViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Deletes a transaction for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the transaction to delete.</param>
    /// <returns>No content if deletion is successful.</returns>
    /// <response code="204">If the transaction is successfully deleted.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="400">If deletion fails.</response>
    /// <response code="404">If the transaction is not found or does not belong to the user.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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