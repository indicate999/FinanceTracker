using FinanceTracker.Models.Finance;

namespace FinanceTracker.BusinessLogic.Interfaces.Repositories;

public interface ITransactionRepository : IBaseRepository<Transaction>
{
    IQueryable<Transaction> GetTransactionsForUser(string userId);
    Task<Transaction?> GetTransactionWithCategoryAsync(int id, string userId);
    Task<bool> HasTransactionsInCategory(int categoryId, string userId);
}