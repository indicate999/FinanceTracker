using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinanceTracker.BusinessLogic.Interfaces.Services;
using FinanceTracker.Controllers;
using FinanceTracker.BusinessLogic.DTOs.Transaction;

namespace FinanceTracker.Tests.Controllers;

public class TransactionControllerTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly TransactionController _transactionController;
    private readonly string _testUserId = "test-user-id";

    public TransactionControllerTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _transactionController = new TransactionController(_mockTransactionService.Object);

        // Setup a mock HttpContext.User for the controller
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
        }, "mock"));

        _transactionController.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetTransactions_ReturnsOkResultWithTransactions()
    {
        // Arrange
        var transactions = new List<TransactionViewDto>
        {
            new TransactionViewDto { Id = 1, Amount = 100, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryName = "Groceries", CategoryId = 1 },
            new TransactionViewDto { Id = 2, Amount = 200, Type = Models.Enums.TransactionType.Income, Date = System.DateTime.UtcNow, CategoryName = "Salary", CategoryId = 2 }
        };
        _mockTransactionService.Setup(s => s.GetTransactionsAsync(_testUserId, "date", "desc"))
                               .ReturnsAsync(transactions);

        // Act
        var result = await _transactionController.GetTransactions("date", "desc");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<TransactionViewDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());
        _mockTransactionService.Verify(s => s.GetTransactionsAsync(_testUserId, "date", "desc"), Times.Once);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsOkResultWithTransaction_WhenTransactionExists()
    {
        // Arrange
        var transactionId = 1;
        var transaction = new TransactionViewDto { Id = transactionId, Amount = 100, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryName = "Groceries", CategoryId = 1 };
        _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(transactionId, _testUserId))
                               .ReturnsAsync(transaction);

        // Act
        var result = await _transactionController.GetTransactionById(transactionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<TransactionViewDto>(okResult.Value);
        Assert.Equal(transactionId, returnValue.Id);
        _mockTransactionService.Verify(s => s.GetTransactionByIdAsync(transactionId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenTransactionDoesNotExist()
    {
        // Arrange
        _mockTransactionService.Setup(s => s.GetTransactionByIdAsync(It.IsAny<int>(), It.IsAny<string>()))
                               .ReturnsAsync((TransactionViewDto?)null);

        // Act
        var result = await _transactionController.GetTransactionById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockTransactionService.Verify(s => s.GetTransactionByIdAsync(99, _testUserId), Times.Once);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsOkResultWithCreatedTransaction()
    {
        // Arrange
        var newTransactionDto = new TransactionDto { Amount = 150, Type = Models.Enums.TransactionType.Income, Date = System.DateTime.UtcNow, CategoryId = 1 };
        var createdTransactionViewDto = new TransactionViewDto { Id = 3, Amount = 150, Type = Models.Enums.TransactionType.Income, Date = System.DateTime.UtcNow, CategoryName = "Salary", CategoryId = 1 };
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(_testUserId, newTransactionDto))
                               .ReturnsAsync((createdTransactionViewDto, null));

        // Act
        var result = await _transactionController.CreateTransaction(newTransactionDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<TransactionViewDto>(okResult.Value);
        Assert.Equal(createdTransactionViewDto.Amount, returnValue.Amount);
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(_testUserId, newTransactionDto), Times.Once);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var newTransactionDto = new TransactionDto { Amount = 150, Type = Models.Enums.TransactionType.Income, Date = System.DateTime.UtcNow, CategoryId = 0 };
        var errorMessage = "Default category not found.";
        _mockTransactionService.Setup(s => s.CreateTransactionAsync(_testUserId, newTransactionDto))
                               .ReturnsAsync((null, errorMessage));

        // Act
        var result = await _transactionController.CreateTransaction(newTransactionDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(_testUserId, newTransactionDto), Times.Once);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsOkResultWithUpdatedTransaction_OnSuccess()
    {
        // Arrange
        var transactionId = 1;
        var updateDto = new TransactionDto { Amount = 120, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryId = 1 };
        var updatedTransactionViewDto = new TransactionViewDto { Id = transactionId, Amount = 120, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryId = 0 };
        _mockTransactionService.Setup(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto))
                               .ReturnsAsync((updatedTransactionViewDto, null));

        // Act
        var result = await _transactionController.UpdateTransaction(transactionId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<TransactionViewDto>(okResult.Value);
        Assert.Equal(updatedTransactionViewDto.Amount, returnValue.Amount);
        _mockTransactionService.Verify(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var transactionId = 1;
        var updateDto = new TransactionDto { Amount = 120, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryId = 1 };
        var errorMessage = "Category type does not match transaction type.";
        _mockTransactionService.Setup(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto))
                               .ReturnsAsync((null, errorMessage));

        // Act
        var result = await _transactionController.UpdateTransaction(transactionId, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        _mockTransactionService.Verify(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        // Arrange
        var transactionId = 1;
        var updateDto = new TransactionDto { Amount = 120, Type = Models.Enums.TransactionType.Expense, Date = System.DateTime.UtcNow, CategoryId = 1 };
        var errorMessage = "Transaction not found or unauthorized.";
        _mockTransactionService.Setup(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto))
                               .ReturnsAsync((null, errorMessage));

        // Act
        var result = await _transactionController.UpdateTransaction(transactionId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMessage, notFoundResult.Value);
        _mockTransactionService.Verify(s => s.UpdateTransactionAsync(transactionId, _testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsNoContent_OnSuccess()
    {
        // Arrange
        var transactionId = 1;
        _mockTransactionService.Setup(s => s.DeleteTransactionAsync(transactionId, _testUserId))
                               .ReturnsAsync((true, null));

        // Act
        var result = await _transactionController.DeleteTransaction(transactionId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockTransactionService.Verify(s => s.DeleteTransactionAsync(transactionId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsBadRequest_OnServiceFailure()
    {
        // Arrange
        var transactionId = 1;
        var errorMessage = "Cannot delete transaction.";
        _mockTransactionService.Setup(s => s.DeleteTransactionAsync(transactionId, _testUserId))
                               .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _transactionController.DeleteTransaction(transactionId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errorMessage, badRequestResult.Value);
        _mockTransactionService.Verify(s => s.DeleteTransactionAsync(transactionId, _testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        // Arrange
        var transactionId = 1;
        var errorMessage = "Transaction not found or unauthorized.";
        _mockTransactionService.Setup(s => s.DeleteTransactionAsync(transactionId, _testUserId))
                               .ReturnsAsync((false, errorMessage));

        // Act
        var result = await _transactionController.DeleteTransaction(transactionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMessage, notFoundResult.Value);
        _mockTransactionService.Verify(s => s.DeleteTransactionAsync(transactionId, _testUserId), Times.Once);
    }
}
