using ExpenseTracker.Api.Data.Repositories;

namespace ExpenseTracker.Api.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IExpenseRepository Expenses { get; }
    Task<int> SaveChangesAsync();
    Task<bool> SaveChangesReturnBoolAsync();
}