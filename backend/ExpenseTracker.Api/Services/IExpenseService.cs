using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(Guid userId);
    Task<ExpenseDto?> GetExpenseByIdAsync(int id, Guid userId);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto, Guid userId);
    Task<ExpenseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto, Guid userId);
    Task<bool> DeleteExpenseAsync(int id, Guid userId);
    Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, Guid userId);
    Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync(Guid userId);
    Task<decimal> GetTotalExpensesAsync(Guid userId);
}