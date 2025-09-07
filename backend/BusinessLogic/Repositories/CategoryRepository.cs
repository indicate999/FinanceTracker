using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.Data;
using FinanceTracker.Models.Finance;
using FinanceTracker.Utils;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.BusinessLogic.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Category?> GetCategoryWithTransactionsAsync(int id, string userId)
    {
        return await _dbSet
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<Category?> GetDefaultCategoryAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == Constants.DefaultCategoryName);
    }

    public IQueryable<Category> GetCategoriesForUser(string userId)
    {
        return _dbSet.Where(c => c.UserId == userId);
    }
}