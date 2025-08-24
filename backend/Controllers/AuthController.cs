using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.BusinessLogic.DTOs.Auth;
using FinanceTracker.Data;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Finance;
using FinanceTracker.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FinanceTracker.Utils;

namespace FinanceTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;

    public AuthController(UserManager<ApplicationUser> userManager,
                          IConfiguration config,
                          SignInManager<ApplicationUser> signInManager,
                          ApplicationDbContext db)
    {
        _userManager = userManager;
        _config = config;
        _signInManager = signInManager;
        _db = db;
        
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                });

            return BadRequest(errors);
        }
        
        var user = new ApplicationUser
        {
            UserName = dto.Username
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        
        var profile = new UserProfile
        {
            UserId = user.Id,
            User = user,
            DisplayName = dto.DisplayName
        };

        _db.UserProfiles.Add(profile);
        
        var uncategorized = new Category
        {
            Name = Constants.DefaultCategoryName,
            Type = CategoryType.Neutral,
            UserId = user.Id
        };
        _db.Categories.Add(uncategorized);

        
        await _db.SaveChangesAsync();

        return Ok(new { message = "User created" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
            return Unauthorized("Invalid login or password");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid login or password");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
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