using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.Data;
using FinanceTracker.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.BusinessLogic.Repositories;

public class UserProfileRepository : BaseRepository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<UserProfile?> GetUserProfileByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(up => up.UserId == userId);
    }
}