using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction;
using FinanceTracker.Models.Finance;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryViewDto>> GetCategoriesAsync(string userId, string sortBy, string sortOrder);
    Task<IReadOnlyList<TransactionViewDto>> GetTransactionsByCategoryAsync(int categoryId, string userId, int skip, int take);
    Task<CategoryViewDto?> GetCategoryByIdAsync(int categoryId, string userId);
    Task<CategoryViewDto> CreateCategoryAsync(string userId, CategoryDto dto);
    Task<(bool Success, string? Error)> UpdateCategoryAsync(int categoryId, string userId, CategoryDto dto);
    Task<(bool Success, string? Error)> DeleteCategoryAsync(int categoryId, string userId);
}