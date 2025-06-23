using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync();
    Task<ExpenseDto?> GetExpenseByIdAsync(int id);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto);
    Task<ExpenseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto);
    Task<bool> DeleteExpenseAsync(int id);
    Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month);
    Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync();
    Task<decimal> GetTotalExpensesAsync();
}