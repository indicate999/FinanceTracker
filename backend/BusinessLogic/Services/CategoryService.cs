using AutoMapper;
using AutoMapper.QueryableExtensions;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using FinanceTracker.Utils;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryViewDto>> GetCategoriesAsync(string userId, string sortBy, string sortOrder)
    {
        var query = _categoryRepository.GetCategoriesForUser(userId)
            .Include(c => c.Transactions) // Still need to include for TransactionCount
            .ProjectTo<CategoryViewDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();

        bool isAscending = sortOrder.ToLower() == "asc";
        query = (sortBy.ToLower()) switch
        {
            "type" => isAscending ? query.OrderBy(c => c.Type) : query.OrderByDescending(c => c.Type),
            _ => isAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
        };

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<TransactionViewDto>> GetTransactionsByCategoryAsync(int categoryId, string userId, int skip, int take)
    {
        var categoryOwns = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
        if (!categoryOwns.Any()) return null!; // Or throw an exception for not found/unauthorized

        var pageSize = Math.Clamp(take, 1, 200);

        var dtos = await _transactionRepository.GetTransactionsForUser(userId)
            .Where(t => t.CategoryId == categoryId)
            .OrderBy(t => t.Date)
            .Skip(skip)
            .Take(pageSize)
            .AsNoTracking()
            .ProjectTo<TransactionViewDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return dtos;
    }

    public async Task<CategoryViewDto?> GetCategoryByIdAsync(int categoryId, string userId)
    {
        var category = await _categoryRepository.GetCategoryWithTransactionsAsync(categoryId, userId);
        if (category == null) return null;
        return _mapper.Map<CategoryViewDto>(category);
    }

    public async Task<CategoryViewDto> CreateCategoryAsync(string userId, CategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        category.UserId = userId;

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();
        return _mapper.Map<CategoryViewDto>(category);
    }

    public async Task<(bool Success, string? Error)> UpdateCategoryAsync(int categoryId, string userId, CategoryDto dto)
    {
        var category = await _categoryRepository.GetCategoryWithTransactionsAsync(categoryId, userId);
        if (category == null) return (false, "Category not found.");

        if (category.Name == Constants.DefaultCategoryName)
            return (false, "This category cannot be edited.");

        var oldType = category.Type;

        _mapper.Map(dto, category);

        if (category.Type != oldType && category.Type != CategoryType.Neutral)
        {
            var defaultCategory = await _categoryRepository.GetDefaultCategoryAsync(userId);
            if (defaultCategory != null)
            {
                foreach (var transaction in category.Transactions.ToList())
                {
                    if ((category.Type == CategoryType.Income && transaction.Type == TransactionType.Expense) ||
                        (category.Type == CategoryType.Expense && transaction.Type == TransactionType.Income))
                    {
                        transaction.CategoryId = defaultCategory.Id;
                        _transactionRepository.Update(transaction); // Mark as updated
                    }
                }
            }
        }

        _categoryRepository.Update(category); // Mark category as updated
        await _categoryRepository.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int categoryId, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.UserId != userId) return (false, "Category not found.");

        if (category.Name == Constants.DefaultCategoryName)
            return (false, "This category cannot be deleted.");
        
        bool hasTransactions = await _transactionRepository.HasTransactionsInCategory(categoryId, userId);
        if (hasTransactions)
        {
            var defaultCategory = await _categoryRepository.GetDefaultCategoryAsync(userId);
            if (defaultCategory == null)
            {
                // This scenario should ideally not happen if every user gets a default category upon registration
                return (false, "Default category not found for user. Cannot reassign transactions.");
            }

            // Get all transactions linked to the category being deleted
            var transactionsToReassign = await _transactionRepository
                .GetTransactionsForUser(userId)
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync(); // Convert to list to execute query

            // Reassign each transaction to the default category
            foreach (var transaction in transactionsToReassign)
            {
                transaction.CategoryId = defaultCategory.Id;
                _transactionRepository.Update(transaction); // Mark as updated
            }
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();
        return (true, null);
    }
}
