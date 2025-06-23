using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Data.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ExpenseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Expense>> GetExpensesByMonthAsync(int year, int month)
    {
        return await _dbSet
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetExpensesTotalByCategoryAsync()
    {
        return await _dbSet
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Category, x => x.Total);
    }

    public async Task<decimal> GetTotalExpensesAsync()
    {
        return await _dbSet.SumAsync(e => e.Amount);
    }

    public async Task<decimal> GetTotalExpensesByMonthAsync(int year, int month)
    {
        return await _dbSet
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .SumAsync(e => e.Amount);
    }

    public override async Task<IEnumerable<Expense>> GetAllAsync()
    {
        return await _dbSet
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }
}