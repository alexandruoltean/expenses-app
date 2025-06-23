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

    public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync()
    {
        try
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            return expenses.Select(MapToDto);
        }
        catch (Exception ex)
        {
            // Log the exact error for debugging
            throw new Exception($"Error in GetAllExpensesAsync: {ex.Message}", ex);
        }
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        return expense != null ? MapToDto(expense) : null;
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto)
    {
        var expense = new Expense
        {
            Title = createExpenseDto.Title,
            Amount = createExpenseDto.Amount,
            Category = createExpenseDto.Category,
            Description = createExpenseDto.Description,
            Date = createExpenseDto.Date,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Expenses.AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(expense);
    }

    public async Task<ExpenseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
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

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return false;

        _unitOfWork.Expenses.Remove(expense);
        return await _unitOfWork.SaveChangesReturnBoolAsync();
    }

    public async Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month)
    {
        var expenses = await _unitOfWork.Expenses.GetExpensesByMonthAsync(year, month);
        return expenses.Select(MapToDto);
    }

    public async Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync()
    {
        var categoryTotals = await _unitOfWork.Expenses.GetExpensesTotalByCategoryAsync();
        return categoryTotals.Select(ct => new ExpenseCategoryTotalDto
        {
            Category = ct.Key,
            Total = ct.Value
        });
    }

    public async Task<decimal> GetTotalExpensesAsync()
    {
        return await _unitOfWork.Expenses.GetTotalExpensesAsync();
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