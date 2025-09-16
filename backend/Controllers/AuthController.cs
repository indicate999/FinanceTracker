using System.Security.Claims;
using FinanceTracker.BusinessLogic.DTOs.Auth;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers;

/// <summary>
/// Handles user authentication, including registration, login, and account management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">The registration details.</param>
    /// <returns>A message indicating successful registration.</returns>
    /// <response code="200">Returns a success message.</response>
    /// <response code="400">If the input data is invalid or user creation fails.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Authenticates a user and provides a JWT token.
    /// </summary>
    /// <param name="dto">The login credentials.</param>
    /// <returns>A JWT token upon successful authentication.</returns>
    /// <response code="200">Returns the JWT token.</response>
    /// <response code="401">If the username or password is invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (token, error) = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized(error);

        return Ok(new { token });
    }

    /// <summary>
    /// Deletes the authenticated user's account and all associated data.
    /// </summary>
    /// <returns>No content if deletion is successful.</returns>
    /// <response code="204">If the account is successfully deleted.</response>
    /// <response code="401">If the user is unauthorized or their ID is missing from the token.</response>
    /// <response code="400">If account deletion fails.</response>
    /// <response code="404">If the user account is not found.</response>
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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