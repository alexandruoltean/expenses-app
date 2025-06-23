using ExpenseTracker.Api.Data.UnitOfWork;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpenseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(Guid userId)
    {
        try
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            var userExpenses = expenses.Where(e => e.UserId == userId);
            return userExpenses.Select(MapToDto);
        }
        catch (Exception ex)
        {
            // Log the exact error for debugging
            throw new Exception($"Error in GetAllExpensesAsync: {ex.Message}", ex);
        }
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(int id, Guid userId)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null || expense.UserId != userId)
            return null;
        return MapToDto(expense);
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto, Guid userId)
    {
        var expense = new Expense
        {
            Title = createExpenseDto.Title,
            Amount = createExpenseDto.Amount,
            Category = createExpenseDto.Category,
            Description = createExpenseDto.Description,
            Date = createExpenseDto.Date,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _unitOfWork.Expenses.AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(expense);
    }

    public async Task<ExpenseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto, Guid userId)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null || expense.UserId != userId)
            return null;

        if (!string.IsNullOrWhiteSpace(updateExpenseDto.Title))
            expense.Title = updateExpenseDto.Title;

        if (updateExpenseDto.Amount.HasValue)
            expense.Amount = updateExpenseDto.Amount.Value;

        if (!string.IsNullOrWhiteSpace(updateExpenseDto.Category))
            expense.Category = updateExpenseDto.Category;

        if (updateExpenseDto.Description != null)
            expense.Description = updateExpenseDto.Description;

        if (updateExpenseDto.Date.HasValue)
            expense.Date = updateExpenseDto.Date.Value;

        expense.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int id, Guid userId)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null || expense.UserId != userId)
            return false;

        _unitOfWork.Expenses.Remove(expense);
        return await _unitOfWork.SaveChangesReturnBoolAsync();
    }

    public async Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, Guid userId)
    {
        var expenses = await _unitOfWork.Expenses.GetExpensesByMonthAsync(year, month);
        var userExpenses = expenses.Where(e => e.UserId == userId);
        return userExpenses.Select(MapToDto);
    }

    public async Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync(Guid userId)
    {
        var allExpenses = await _unitOfWork.Expenses.GetAllAsync();
        var userExpenses = allExpenses.Where(e => e.UserId == userId);
        var categoryTotals = userExpenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        
        return categoryTotals.Select(ct => new ExpenseCategoryTotalDto
        {
            Category = ct.Key,
            Total = ct.Value
        });
    }

    public async Task<decimal> GetTotalExpensesAsync(Guid userId)
    {
        var allExpenses = await _unitOfWork.Expenses.GetAllAsync();
        var userExpenses = allExpenses.Where(e => e.UserId == userId);
        return userExpenses.Sum(e => e.Amount);
    }

    private static ExpenseDto MapToDto(Expense expense)
    {
        return new ExpenseDto
        {
            Id = expense.Id,
            Title = expense.Title,
            Amount = expense.Amount,
            Category = expense.Category,
            Description = expense.Description,
            Date = expense.Date,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }
}