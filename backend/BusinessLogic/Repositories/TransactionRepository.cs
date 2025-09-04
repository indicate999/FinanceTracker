using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.Data;
using FinanceTracker.Models.Finance;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.BusinessLogic.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }

    public IQueryable<Transaction> GetTransactionsForUser(string userId)
    {
        return _dbSet.Where(t => t.UserId == userId);
    }

    public async Task<Transaction?> GetTransactionWithCategoryAsync(int id, string userId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<bool> HasTransactionsInCategory(int categoryId, string userId)
    {
        return await _dbSet.AnyAsync(t => t.CategoryId == categoryId && t.UserId == userId);
    }
}