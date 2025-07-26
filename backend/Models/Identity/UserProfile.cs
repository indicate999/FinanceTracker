using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models.Identity;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public required ApplicationUser User { get; set; }
    
    public required string DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}