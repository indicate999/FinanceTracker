using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.BusinessLogic.DTOs.Auth;
using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using FinanceTracker.Models.Identity;
using FinanceTracker.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTracker.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config,
        IUserProfileRepository userProfileRepository,
        ICategoryRepository categoryRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _userProfileRepository = userProfileRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<(IdentityResult Result, string? Message)> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Username
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (result, null);

        var profile = new UserProfile
        {
            User = user,
            UserId = user.Id,
            DisplayName = dto.DisplayName
        };
        await _userProfileRepository.AddAsync(profile);

        var uncategorized = new Category
        {
            Name = Constants.DefaultCategoryName,
            Type = CategoryType.Neutral,
            UserId = user.Id
        };
        await _categoryRepository.AddAsync(uncategorized);
        
        await _userProfileRepository.SaveChangesAsync(); // Save changes for both profile and category

        return (result, "User created successfully.");
    }

    public async Task<(string? Token, string? Error)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
            return (null, "Invalid login or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return (null, "Invalid login or password.");

        var token = GenerateJwtToken(user);
        return (token, null);
    }

    public async Task<(IdentityResult Result, string? Error)> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (IdentityResult.Failed(), "User not found.");

        var result = await _userManager.DeleteAsync(user);
        
        // Due to cascade delete configured in DbContext, associated UserProfile, Categories, and Transactions
        // will be deleted automatically by the database. No explicit repository calls needed here for those.

        if (!result.Succeeded)
        {
            return (result, "Failed to delete account.");
        }
        
        return (result, null);
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("username", user.UserName!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(14),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
