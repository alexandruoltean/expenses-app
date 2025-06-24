using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(Guid userId, int? groupId = null);
    Task<ExpenseDto?> GetExpenseByIdAsync(int id, Guid userId);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto, Guid userId);
    Task<ExpenseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto, Guid userId);
    Task<bool> DeleteExpenseAsync(int id, Guid userId);
    Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, Guid userId, int? groupId = null);
    Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync(Guid userId, int? groupId = null);
    Task<decimal> GetTotalExpensesAsync(Guid userId, int? groupId = null);
}