using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.Interfaces.Services;

public interface ITransactionService
{
    Task<IEnumerable<TransactionViewDto>> GetTransactionsAsync(string userId, string sortBy, string sortOrder);
    Task<TransactionViewDto?> GetTransactionByIdAsync(int transactionId, string userId);
    Task<(TransactionViewDto? Dto, string? Error)> CreateTransactionAsync(string userId, TransactionDto dto);
    Task<(TransactionViewDto? Dto, string? Error)> UpdateTransactionAsync(int transactionId, string userId, TransactionDto dto);
    Task<(bool Success, string? Error)> DeleteTransactionAsync(int transactionId, string userId);
}