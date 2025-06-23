using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
    {
        var expenses = await _expenseService.GetAllExpensesAsync();
        return Ok(expenses);
    }

    /// <summary>
    /// Get expense by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
    {
        try
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
                return NotFound($"Expense with ID {id} not found");

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the expense");
        }
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense(CreateExpenseDto createExpenseDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = await _expenseService.CreateExpenseAsync(createExpenseDto);
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, $"An error occurred while creating the expense: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(int id, UpdateExpenseDto updateExpenseDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = await _expenseService.UpdateExpenseAsync(id, updateExpenseDto);
            if (expense == null)
                return NotFound($"Expense with ID {id} not found");

            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the expense");
        }
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            if (!result)
                return NotFound($"Expense with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the expense");
        }
    }

    /// <summary>
    /// Get expenses by month
    /// </summary>
    [HttpGet("month/{year}/{month}")]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpensesByMonth(int year, int month)
    {
        try
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");

            var expenses = await _expenseService.GetExpensesByMonthAsync(year, month);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses for {Year}/{Month}", year, month);
            return StatusCode(500, "An error occurred while retrieving expenses");
        }
    }

    /// <summary>
    /// Get total expenses by category
    /// </summary>
    [HttpGet("by-category")]
    public async Task<ActionResult<IEnumerable<ExpenseCategoryTotalDto>>> GetExpensesByCategory()
    {
        try
        {
            var categoryTotals = await _expenseService.GetExpensesTotalByCategoryAsync();
            return Ok(categoryTotals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses by category");
            return StatusCode(500, "An error occurred while retrieving expense totals by category");
        }
    }

    /// <summary>
    /// Get total of all expenses
    /// </summary>
    [HttpGet("total")]
    public async Task<ActionResult<decimal>> GetTotalExpenses()
    {
        try
        {
            var total = await _expenseService.GetTotalExpensesAsync();
            return Ok(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total expenses");
            return StatusCode(500, "An error occurred while retrieving total expenses");
        }
    }
}