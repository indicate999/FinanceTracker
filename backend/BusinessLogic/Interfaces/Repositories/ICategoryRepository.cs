using FinanceTracker.Models.Finance;

namespace FinanceTracker.BusinessLogic.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetCategoryWithTransactionsAsync(int id, string userId);
    Task<Category?> GetDefaultCategoryAsync(string userId);
    IQueryable<Category> GetCategoriesForUser(string userId);
}