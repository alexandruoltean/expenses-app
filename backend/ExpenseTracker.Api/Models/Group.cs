using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Models;

public class Group
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string InviteCode { get; set; } = string.Empty;
    
    public Guid CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}