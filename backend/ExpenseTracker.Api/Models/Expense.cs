using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Models;

public class Expense
{
    public int Id { get; set; }
    
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key for User
    [Required]
    public Guid UserId { get; set; }
    
    // Navigation property
    public virtual User? User { get; set; }
}