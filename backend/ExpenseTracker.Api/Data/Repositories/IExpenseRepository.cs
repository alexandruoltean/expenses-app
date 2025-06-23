using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Data.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IEnumerable<Expense>> GetExpensesByMonthAsync(int year, int month);
    Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(string category);
    Task<Dictionary<string, decimal>> GetExpensesTotalByCategoryAsync();
    Task<decimal> GetTotalExpensesAsync();
    Task<decimal> GetTotalExpensesByMonthAsync(int year, int month);
}