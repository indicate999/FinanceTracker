using FinanceTracker.Models.Identity;

namespace FinanceTracker.BusinessLogic.Interfaces.Repositories;

public interface IUserProfileRepository : IGenericRepository<UserProfile>
{
    Task<UserProfile?> GetUserProfileByUserIdAsync(string userId);
}