using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Auth;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (result, message) = await _authService.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        return Ok(new { message = message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (token, error) = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized(error);

        return Ok(new { token });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        var (result, error) = await _authService.DeleteAccountAsync(userId);
        if (!result.Succeeded)
        {
            return BadRequest(error);
        }
        
        return NoContent();
    }
}