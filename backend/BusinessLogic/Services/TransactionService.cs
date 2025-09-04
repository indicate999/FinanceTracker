using AutoMapper;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.BusinessLogic.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionViewDto>> GetTransactionsAsync(string userId, string sortBy, string sortOrder)
    {
        var query = _transactionRepository.GetTransactionsForUser(userId)
            .Include(t => t.Category) // Include category for display
            .AsNoTracking(); // Good practice for read-only operations

        bool isAscending = sortOrder.ToLower() == "asc";
        query = (sortBy.ToLower()) switch
        {
            "amount" => isAscending ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
            "type" => isAscending ? query.OrderBy(t => t.Type) : query.OrderByDescending(t => t.Type),
            "date" => isAscending ? query.OrderBy(t => t.Date) : query.OrderByDescending(t => t.Date),
            "category" => isAscending ? query.OrderBy(t => t.Category.Name) : query.OrderByDescending(t => t.Category.Name),
            _ => isAscending ? query.OrderBy(t => t.Date) : query.OrderByDescending(t => t.Date),
        };

        var transactions = await query.ToListAsync();
        return _mapper.Map<List<TransactionViewDto>>(transactions);
    }

    public async Task<TransactionViewDto?> GetTransactionByIdAsync(int transactionId, string userId)
    {
        var transaction = await _transactionRepository.GetTransactionWithCategoryAsync(transactionId, userId);
        if (transaction == null) return null;
        return _mapper.Map<TransactionViewDto>(transaction);
    }

    public async Task<(TransactionViewDto? Dto, string? Error)> CreateTransactionAsync(string userId, TransactionDto dto)
    {
        int categoryId = dto.CategoryId;

        if (categoryId == 0)
        {
            var uncategorized = await _categoryRepository.GetDefaultCategoryAsync(userId);
            if (uncategorized == null)
                return (null, "Default category not found.");
            categoryId = uncategorized.Id;
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.UserId != userId)
            return (null, "Category not found or unauthorized.");

        if (!IsCompatible(dto.Type, category.Type))
            return (null, "Category type does not match transaction type.");

        var transaction = _mapper.Map<Transaction>(dto);
        transaction.CategoryId = categoryId;
        transaction.UserId = userId;
        transaction.Date = transaction.Date.ToUniversalTime();

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // After saving, reload with category for DTO mapping if needed
        transaction = await _transactionRepository.GetTransactionWithCategoryAsync(transaction.Id, userId);
        return (_mapper.Map<TransactionViewDto>(transaction), null);
    }

    public async Task<(TransactionViewDto? Dto, string? Error)> UpdateTransactionAsync(int transactionId, string userId, TransactionDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.UserId != userId)
            return (null, "Transaction not found or unauthorized.");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || category.UserId != userId)
            return (null, "Category not found or unauthorized.");

        if (!IsCompatible(dto.Type, category.Type))
            return (null, "Category type does not match transaction type.");

        _mapper.Map(dto, transaction);
        transaction.Date = transaction.Date.ToUniversalTime();

        _transactionRepository.Update(transaction);
        await _transactionRepository.SaveChangesAsync();
        
        // After saving changes, get the transaction with its category to correctly map to TransactionViewDto
        // This ensures Category.Name is populated for the frontend
        var updatedTransaction = await _transactionRepository.GetTransactionWithCategoryAsync(transaction.Id, userId);
        if (updatedTransaction == null)
        {
            // Should theoretically not happen if SaveChangesAsync() succeeded, but as a safeguard
            return (null, "Failed to retrieve updated transaction.");
        }
        
        return (_mapper.Map<TransactionViewDto>(updatedTransaction), null);
    }

    public async Task<(bool Success, string? Error)> DeleteTransactionAsync(int transactionId, string userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.UserId != userId)
            return (false, "Transaction not found or unauthorized.");

        _transactionRepository.Remove(transaction);
        await _transactionRepository.SaveChangesAsync();
        return (true, null);
    }

    private bool IsCompatible(TransactionType transactionType, CategoryType categoryType)
    {
        return categoryType == CategoryType.Neutral
               || (transactionType == TransactionType.Income && categoryType == CategoryType.Income)
               || (transactionType == TransactionType.Expense && categoryType == CategoryType.Expense);
    }
}
