
using FinanceTracker.Models.Enums;

namespace FinanceTracker.BusinessLogic.DTOs.Category;

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
}