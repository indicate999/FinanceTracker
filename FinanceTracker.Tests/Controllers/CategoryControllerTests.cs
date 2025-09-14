using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Controllers;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.BusinessLogic.DTOs.Transaction; // Needed for GetTransactionsByCategory

namespace FinanceTracker.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoryController _categoryController;
    private readonly string _testUserId = "test-user-id";

    public CategoryControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _categoryController = new CategoryController(_mockCategoryService.Object);

        // Setup a mock HttpContext.User for the controller to simulate an authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
        }, "mock"));

        _categoryController.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetCategories_ReturnsOkResultWithCategories()
    {
        // Arrange
        var categories = new List<CategoryViewDto>
        {
            new CategoryViewDto { Id = 1, Name = "Food", Type = Models.Enums.CategoryType.Expense, TransactionCount = 5 },
            new CategoryViewDto { Id = 2, Name = "Salary", Type = Models.Enums.CategoryType.Income, TransactionCount = 10 }
        };
        _mockCategoryService.Setup(s => s.GetCategoriesAsync(_testUserId, "name", "asc"))
                            .ReturnsAsync(categories);

        // Act
        var result = await _categoryController.GetCategories("name", "asc");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<CategoryViewDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());
        _mockCategoryService.Verify(s => s.GetCategoriesAsync(_testUserId, "name", "asc"), Times.Once);
    }

    [Fact]
    public async Task GetCategories_ReturnsUnauthorized_WhenUserIdIsMissing()
    {
        // Arrange - Remove the user from HttpContext to simulate unauthenticated
        _categoryController.ControllerContext.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _categoryController.GetCategories("name", "asc");

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
        _mockCategoryService.Verify(s => s.GetCategoriesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOkResultWithCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = 1;
        var category = new CategoryViewDto { Id = categoryId, Name = "Food", Type = Models.Enums.CategoryType.Expense, TransactionCount = 5 };
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(categoryId, _testUserId))
                            .ReturnsAsync(category);

        // Act
        var result = await _categoryController.GetCategoryById(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<CategoryViewDto>(okResult.Value);
        Assert.Equal(categoryId, returnValue.Id);
        _mockCategoryService.Verify(s => s.GetCategoryByIdAsync(categoryId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                            .ReturnsAsync((CategoryViewDto?)null);

        // Act
        var result = await _categoryController.GetCategoryById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockCategoryService.Verify(s => s.GetCategoryByIdAsync(99, _testUserId), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ReturnsOkResultWithCreatedCategory()
    {
        // Arrange
        var newCategoryDto = new CategoryDto { Name = "Test", Type = Models.Enums.CategoryType.Expense };
        var createdCategoryViewDto = new CategoryViewDto { Id = 3, Name = "Test", Type = Models.Enums.CategoryType.Expense, TransactionCount = 0 };
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(_testUserId, newCategoryDto))
                            .ReturnsAsync(createdCategoryViewDto);

        // Act
        var result = await _categoryController.CreateCategory(newCategoryDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<CategoryViewDto>(okResult.Value);
        Assert.Equal(createdCategoryViewDto.Name, returnValue.Name);
        _mockCategoryService.Verify(s => s.CreateCategoryAsync(_testUserId, newCategoryDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNoContentResult_OnSuccess()
    {
        // Arrange
        var categoryId = 1;
        var updateDto = new CategoryDto { Name = "Updated Name", Type = Models.Enums.CategoryType.Income };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto))
                            .ReturnsAsync((true, null));

        // Act
        var result = await _categoryController.UpdateCategory(categoryId, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCategoryService.Verify(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var categoryId = 1;
        var updateDto = new CategoryDto { Name = "Updated Name", Type = Models.Enums.CategoryType.Income };
        var errorMessage = "This category cannot be edited.";
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto))
                            .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _categoryController.UpdateCategory(categoryId, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        _mockCategoryService.Verify(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        // Arrange
        var categoryId = 1;
        var updateDto = new CategoryDto { Name = "Updated Name", Type = Models.Enums.CategoryType.Income };
        var errorMessage = "Category not found.";
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto))
                            .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _categoryController.UpdateCategory(categoryId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMessage, notFoundResult.Value);
        _mockCategoryService.Verify(s => s.UpdateCategoryAsync(categoryId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        var categoryId = 1;
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId, _testUserId))
                            .ReturnsAsync((true, null));

        // Act
        var result = await _categoryController.DeleteCategory(categoryId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCategoryService.Verify(s => s.DeleteCategoryAsync(categoryId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var categoryId = 1;
        var errorMessage = "This category cannot be deleted.";
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId, _testUserId))
                            .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _categoryController.DeleteCategory(categoryId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        _mockCategoryService.Verify(s => s.DeleteCategoryAsync(categoryId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        // Arrange
        var categoryId = 1;
        var errorMessage = "Category not found.";
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId, _testUserId))
                            .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _categoryController.DeleteCategory(categoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMessage, notFoundResult.Value);
        _mockCategoryService.Verify(s => s.DeleteCategoryAsync(categoryId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByCategory_ReturnsOkResultWithTransactions()
    {
        // Arrange
        var categoryId = 1;
        var transactions = new List<TransactionViewDto>
        {
            new TransactionViewDto { Id = 101, Amount = 100, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryName = "Groceries", CategoryId = 1 },
            new TransactionViewDto { Id = 102, Amount = 50, Type = Models.Enums.TransactionType.Income, Date = System.DateTime.UtcNow, CategoryName = "Salary", CategoryId = 2 }
        };
        _mockCategoryService.Setup(s => s.GetTransactionsByCategoryAsync(categoryId, _testUserId, 0, 50))
                            .ReturnsAsync(transactions);

        // Act
        var result = await _categoryController.GetTransactionsByCategory(categoryId, 0, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IReadOnlyList<TransactionViewDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
        _mockCategoryService.Verify(s => s.GetTransactionsByCategoryAsync(categoryId, _testUserId, 0, 50), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByCategory_ReturnsNotFound_WhenCategoryServiceReturnsNull()
    {
        // Arrange
        var categoryId = 1;
        _mockCategoryService.Setup(s => s.GetTransactionsByCategoryAsync(categoryId, _testUserId, 0, 50))
                            .ReturnsAsync((IReadOnlyList<TransactionViewDto>?)null);

        // Act
        var result = await _categoryController.GetTransactionsByCategory(categoryId, 0, 50);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockCategoryService.Verify(s => s.GetTransactionsByCategoryAsync(categoryId, _testUserId, 0, 50), Times.Once);
    }
}
