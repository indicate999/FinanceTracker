
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.DTOs.Category;

public class CategoryViewDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public int TransactionCount { get; set; }
}