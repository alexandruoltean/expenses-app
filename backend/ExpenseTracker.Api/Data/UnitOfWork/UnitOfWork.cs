using ExpenseTracker.Api.Data.Repositories;

namespace ExpenseTracker.Api.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ExpenseContext _context;
    private IExpenseRepository? _expenseRepository;

    public UnitOfWork(ExpenseContext context)
    {
        _context = context;
    }

    public IExpenseRepository Expenses => 
        _expenseRepository ??= new ExpenseRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        // Update timestamps
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Models.Expense expense)
            {
                expense.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesReturnBoolAsync()
    {
        var result = await SaveChangesAsync();
        return result > 0;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}