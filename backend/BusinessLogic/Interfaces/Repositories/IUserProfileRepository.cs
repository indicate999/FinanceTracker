using FinanceTracker.Models.Identity;

namespace FinanceTracker.BusinessLogic.Interfaces.Repositories;

public interface IUserProfileRepository : IBaseRepository<UserProfile>
{
    Task<UserProfile?> GetUserProfileByUserIdAsync(string userId);
}