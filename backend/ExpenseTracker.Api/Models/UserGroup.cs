using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Models;

public class UserGroup
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Member"; // Member, Admin
}