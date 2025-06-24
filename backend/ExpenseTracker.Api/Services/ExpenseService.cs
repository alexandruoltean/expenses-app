using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Data.UnitOfWork;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExpenseContext _context;
    private readonly IGroupService _groupService;

    public ExpenseService(IUnitOfWork unitOfWork, ExpenseContext context, IGroupService groupService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _groupService = groupService;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(Guid userId, int? groupId = null)
    {
        try
        {
            var query = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Group)
                .AsQueryable();

            if (groupId.HasValue)
            {
                // For group expenses, check if user is in the group
                var isUserInGroup = await _groupService.IsUserInGroupAsync(userId, groupId.Value);
                if (!isUserInGroup)
                    return new List<ExpenseDto>();
                
                query = query.Where(e => e.GroupId == groupId.Value);
            }
            else
            {
                // For personal expenses, get expenses without a group for this user
                query = query.Where(e => e.UserId == userId && e.GroupId == null);
            }

            var expenses = await query.ToListAsync();
            return expenses.Select(MapToDto);
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
        // If groupId is provided, verify user is in the group
        if (createExpenseDto.GroupId.HasValue)
        {
            var isUserInGroup = await _groupService.IsUserInGroupAsync(userId, createExpenseDto.GroupId.Value);
            if (!isUserInGroup)
                throw new UnauthorizedAccessException("User is not a member of the specified group");
        }

        var expense = new Expense
        {
            Title = createExpenseDto.Title,
            Amount = createExpenseDto.Amount,
            Category = createExpenseDto.Category,
            Description = createExpenseDto.Description,
            Date = createExpenseDto.Date,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId,
            GroupId = createExpenseDto.GroupId
        };

        await _unitOfWork.Expenses.AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        // Load related data for DTO mapping
        var expenseWithIncludes = await _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Group)
            .FirstOrDefaultAsync(e => e.Id == expense.Id);

        return MapToDto(expenseWithIncludes ?? expense);
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

    public async Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, Guid userId, int? groupId = null)
    {
        var query = _context.Expenses
            .Include(e => e.User)
            .Include(e => e.Group)
            .Where(e => e.Date.Year == year && e.Date.Month == month);

        if (groupId.HasValue)
        {
            var isUserInGroup = await _groupService.IsUserInGroupAsync(userId, groupId.Value);
            if (!isUserInGroup)
                return new List<ExpenseDto>();
            
            query = query.Where(e => e.GroupId == groupId.Value);
        }
        else
        {
            query = query.Where(e => e.UserId == userId && e.GroupId == null);
        }

        var expenses = await query.ToListAsync();
        return expenses.Select(MapToDto);
    }

    public async Task<IEnumerable<ExpenseCategoryTotalDto>> GetExpensesTotalByCategoryAsync(Guid userId, int? groupId = null)
    {
        var query = _context.Expenses.AsQueryable();

        if (groupId.HasValue)
        {
            var isUserInGroup = await _groupService.IsUserInGroupAsync(userId, groupId.Value);
            if (!isUserInGroup)
                return new List<ExpenseCategoryTotalDto>();
            
            query = query.Where(e => e.GroupId == groupId.Value);
        }
        else
        {
            query = query.Where(e => e.UserId == userId && e.GroupId == null);
        }

        var categoryTotals = await query
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryTotalDto
            {
                Category = g.Key,
                Total = g.Sum(e => e.Amount)
            })
            .ToListAsync();
        
        return categoryTotals;
    }

    public async Task<decimal> GetTotalExpensesAsync(Guid userId, int? groupId = null)
    {
        var query = _context.Expenses.AsQueryable();

        if (groupId.HasValue)
        {
            var isUserInGroup = await _groupService.IsUserInGroupAsync(userId, groupId.Value);
            if (!isUserInGroup)
                return 0;
            
            query = query.Where(e => e.GroupId == groupId.Value);
        }
        else
        {
            query = query.Where(e => e.UserId == userId && e.GroupId == null);
        }

        return await query.SumAsync(e => e.Amount);
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
            UpdatedAt = expense.UpdatedAt,
            GroupId = expense.GroupId,
            GroupName = expense.Group?.Name,
            CreatedByUsername = expense.User?.Username ?? ""
        };
    }
}