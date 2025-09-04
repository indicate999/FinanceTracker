using FinanceTracker.BusinessLogic.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.BusinessLogic.Interfaces.Services;

public interface IAuthService
{
    Task<(IdentityResult Result, string? Message)> RegisterAsync(RegisterDto dto);
    Task<(string? Token, string? Error)> LoginAsync(LoginDto dto);
    Task<(IdentityResult Result, string? Error)> DeleteAccountAsync(string userId);
}