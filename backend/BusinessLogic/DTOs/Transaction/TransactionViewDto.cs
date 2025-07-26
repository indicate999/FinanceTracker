
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.DTOs.Transaction;

public class TransactionViewDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}