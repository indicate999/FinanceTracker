
using System.ComponentModel.DataAnnotations.Schema;
using FinanceTracker.Models.Enums;
using FinanceTracker.Models.Identity;

namespace FinanceTracker.Models.Finance;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;
    
    public string UserId { get; set; } = null!;
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}