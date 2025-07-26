
using System.ComponentModel.DataAnnotations.Schema;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Identity;

namespace FinanceTracker.Models.Finance;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    
    public string UserId { get; set; } = null!;
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}