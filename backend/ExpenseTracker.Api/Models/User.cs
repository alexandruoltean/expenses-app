using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Salt { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? LastLoginAt { get; set; }

        // Navigation property
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}