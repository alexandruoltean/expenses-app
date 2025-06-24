using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
}

public class CreateExpenseDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public int? GroupId { get; set; }
}

public class UpdateExpenseDto
{
    [StringLength(100)]
    public string? Title { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime? Date { get; set; }
}

public class ExpenseCategoryTotalDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}