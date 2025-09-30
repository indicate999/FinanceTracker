
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceTracker.BusinessLogic.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "UserName must be 1–20 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be 6–20 characters")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\d]).{6,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one special character."
    )]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "DisplayName must be 1–20 characters")]
    public string DisplayName { get; set; } = string.Empty;
}