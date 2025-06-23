using Xunit;
using Moq;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Api.Data.UnitOfWork;
using ExpenseTracker.Api.Data.Repositories;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Tests.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IExpenseRepository> _mockExpenseRepository;
    private readonly ExpenseService _expenseService;

    public ExpenseServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExpenseRepository = new Mock<IExpenseRepository>();
        _mockUnitOfWork.Setup(x => x.Expenses).Returns(_mockExpenseRepository.Object);
        _expenseService = new ExpenseService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllExpensesAsync_ShouldReturnAllExpenses()
    {
        // Arrange
        var expenses = new List<Expense>
        {
            new() { Id = 1, Title = "Test 1", Amount = 100, Category = "Food", Date = DateTime.Now },
            new() { Id = 2, Title = "Test 2", Amount = 200, Category = "Transport", Date = DateTime.Now }
        };
        _mockExpenseRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(expenses);

        // Act
        var result = await _expenseService.GetAllExpensesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Test 1", result.First().Title);
    }

    [Fact]
    public async Task CreateExpenseAsync_ShouldCreateAndReturnExpense()
    {
        // Arrange
        var createDto = new CreateExpenseDto
        {
            Title = "New Expense",
            Amount = 150,
            Category = "Food",
            Date = DateTime.Now
        };

        var expectedExpense = new Expense
        {
            Id = 1,
            Title = createDto.Title,
            Amount = createDto.Amount,
            Category = createDto.Category,
            Date = createDto.Date
        };

        _mockExpenseRepository.Setup(x => x.AddAsync(It.IsAny<Expense>()))
            .ReturnsAsync(expectedExpense);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _expenseService.CreateExpenseAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.Amount, result.Amount);
        Assert.Equal(createDto.Category, result.Category);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ShouldReturnTrue_WhenExpenseExists()
    {
        // Arrange
        var expense = new Expense { Id = 1, Title = "Test", Amount = 100, Category = "Food", Date = DateTime.Now };
        _mockExpenseRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(expense);
        _mockUnitOfWork.Setup(x => x.SaveChangesReturnBoolAsync()).ReturnsAsync(true);

        // Act
        var result = await _expenseService.DeleteExpenseAsync(1);

        // Assert
        Assert.True(result);
        _mockExpenseRepository.Verify(x => x.Remove(expense), Times.Once);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ShouldReturnFalse_WhenExpenseDoesNotExist()
    {
        // Arrange
        _mockExpenseRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Expense?)null);

        // Act
        var result = await _expenseService.DeleteExpenseAsync(1);

        // Assert
        Assert.False(result);
        _mockExpenseRepository.Verify(x => x.Remove(It.IsAny<Expense>()), Times.Never);
    }
}