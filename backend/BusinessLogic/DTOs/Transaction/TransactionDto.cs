
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.DTOs.Transaction;

public class TransactionDto
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public int CategoryId { get; set; }
}