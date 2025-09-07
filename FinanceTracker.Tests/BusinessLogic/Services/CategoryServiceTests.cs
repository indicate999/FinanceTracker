using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceTracker.BusinessLogic.Interfaces.Repositories;
using FinanceTracker.BusinessLogic.Services;
using FinanceTracker.BusinessLogic.DTOs.Category;
using FinanceTracker.Models.Finance;
using AutoMapper;
using FinanceTracker.Utils; // For Constants
using FinanceTracker.Models.Enums; // For CategoryType
using FinanceTracker.BusinessLogic.Mappings; // Add this using directive for your AutoMapper profile
using MockQueryable.Moq; // Add this using directive
using Microsoft.EntityFrameworkCore; // Required for ToListAsync() where used on IQueryable mocks


namespace FinanceTracker.Tests.BusinessLogic.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly IMapper _mapper; // This will be the real AutoMapper instance
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        // REMOVED: _mockMapper = new Mock<IMapper>(); // This is no longer needed as we use the real _mapper

        // Configure a real AutoMapper instance for tests that use ProjectTo or direct .Map() calls
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new CategoryProfile());
            // Add other profiles here if your service method maps other DTOs (e.g., TransactionViewDto)
            // cfg.AddProfile(new TransactionProfile()); // Example if you had one
        }, null); // Pass null for ILoggerFactory if not needed
        _mapper = mapperConfiguration.CreateMapper();

        _categoryService = new CategoryService(
            _mockCategoryRepository.Object,
            _mockTransactionRepository.Object,
            _mapper // Pass the real AutoMapper instance to the service
        );
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnFilteredAndSortedCategories()
    {
        // Arrange
        var userId = "user1";
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Food", Type = CategoryType.Expense, UserId = userId, Transactions = new List<Transaction>() },
            new Category { Id = 2, Name = "Salary", Type = CategoryType.Income, UserId = userId, Transactions = new List<Transaction>() },
            new Category { Id = 3, Name = "Rent", Type = CategoryType.Expense, UserId = userId, Transactions = new List<Transaction>() }
        };

        // Use MockQueryable.Moq to create a mock IQueryable that ProjectTo and LINQ methods can work with
        // We set up both specific and It.IsAny to ensure coverage. The more specific one will be matched if applicable.
        _mockCategoryRepository.Setup(r => r.GetCategoriesForUser(userId))
                               .Returns(categories.AsQueryable().BuildMock().Object); // Use .BuildMock().Object

        _mockCategoryRepository.Setup(r => r.GetCategoriesForUser(It.IsAny<string>()))
                               .Returns(categories.AsQueryable().BuildMock().Object); // Also use .BuildMock().Object if this line is kept and used
        

        // Act
        // The service's GetCategoriesAsync will now call .ProjectTo and .ToListAsync() on the mock IQueryable.
        // Because mockCategoriesQueryable (from BuildMock) behaves like a real queryable,
        // and _mapper is a real instance, ProjectTo will execute correctly.
        var result = await _categoryService.GetCategoriesAsync(userId, "name", "asc");

        // Assert
        Assert.NotNull(result);
        var resultArray = result.ToList();
        Assert.Equal(3, resultArray.Count);
        
        // Verify sorting by name ascending
        Assert.Equal("Food", resultArray[0].Name);
        Assert.Equal("Rent", resultArray[1].Name);
        Assert.Equal("Salary", resultArray[2].Name);

        _mockCategoryRepository.Verify(r => r.GetCategoriesForUser(userId), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldAddCategoryAndSaveChanges()
    {
        // Arrange
        var userId = "user1";
        var newCategoryDto = new CategoryDto { Name = "Savings", Type = CategoryType.Income };
        // Assign an Id to simulate EF Core setting it after AddAsync
        var newCategory = new Category { Id = 1, Name = "Savings", Type = CategoryType.Income, UserId = userId }; 
        
        // Since we are using a real IMapper (_mapper), we don't mock its Map calls.
        // Instead, we ensure the real mapper has the mapping configured via CategoryProfile.
        // The service will directly call _mapper.Map(), and we expect that to work.
        
        // Mock repository calls
        _mockCategoryRepository.Setup(r => r.AddAsync(newCategory)).Returns(Task.CompletedTask);
        _mockCategoryRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _categoryService.CreateCategoryAsync(userId, newCategoryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newCategoryDto.Name, result.Name);
        Assert.Equal(newCategoryDto.Type, result.Type);
        // Assert on the Id assigned by our test newCategory instance
        Assert.Equal(newCategory.Id, result.Id);

        // Verify that the repository methods were called
        _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once); // Use It.IsAny if you don't care about the specific instance passed
        _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        // We don't verify _mockMapper.Verify(m => m.Map...) because we're using a real mapper for this service instance
        // and AutoMapper's internal behavior is not what we're unit testing here.
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReassignTransactionsAndRemoveCategory()
    {
        // Arrange
        var userId = "user1";
        var categoryIdToDelete = 1;
        var defaultCategoryId = 99;

        var categoryToDelete = new Category { Id = categoryIdToDelete, Name = "Old Bills", Type = CategoryType.Expense, UserId = userId };
        var defaultCategory = new Category { Id = defaultCategoryId, Name = Constants.DefaultCategoryName, Type = CategoryType.Neutral, UserId = userId };
        
        var transactions = new List<Transaction>
        {
            new Transaction { Id = 101, CategoryId = categoryIdToDelete, UserId = userId },
            new Transaction { Id = 102, CategoryId = categoryIdToDelete, UserId = userId }
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryIdToDelete)).ReturnsAsync(categoryToDelete);
        _mockCategoryRepository.Setup(r => r.GetDefaultCategoryAsync(userId)).ReturnsAsync(defaultCategory);
        _mockTransactionRepository.Setup(r => r.HasTransactionsInCategory(categoryIdToDelete, userId)).ReturnsAsync(true);
        
        // Use MockQueryable.Moq for IQueryable returns that will be filtered/queried
        _mockTransactionRepository.Setup(r => r.GetTransactionsForUser(userId))
                                  .Returns(transactions.AsQueryable().BuildMock().Object); // Use .BuildMock().Object

        _mockCategoryRepository.Setup(r => r.Remove(categoryToDelete));
        _mockCategoryRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var (success, error) = await _categoryService.DeleteCategoryAsync(categoryIdToDelete, userId);

        // Assert
        Assert.True(success);
        Assert.Null(error);

        _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryIdToDelete), Times.Once);
        _mockTransactionRepository.Verify(r => r.HasTransactionsInCategory(categoryIdToDelete, userId), Times.Once);
        _mockCategoryRepository.Verify(r => r.GetDefaultCategoryAsync(userId), Times.Once);

        // Verify that each transaction's CategoryId was updated and marked for update
        foreach (var transaction in transactions)
        {
            Assert.Equal(defaultCategoryId, transaction.CategoryId);
            _mockTransactionRepository.Verify(r => r.Update(transaction), Times.Once);
        }

        _mockCategoryRepository.Verify(r => r.Remove(categoryToDelete), Times.Once);
        _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    
    // Remember to add more tests for other scenarios and edge cases!
    // For example:
    // - GetCategoryByIdAsync (found, not found)
    // - UpdateCategoryAsync (success, not found, default category, type change logic, etc.)
    // - DeleteCategoryAsync (category not found, not user's category, default category deletion prevented)
    // - DeleteCategoryAsync when no transactions exist in the category
    // - Handling null default category during reassign
}