using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Data;

public class ExpenseContext : DbContext
{
    public ExpenseContext(DbContextOptions<ExpenseContext> options) : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.Date)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");
        });

        // Seed data
        modelBuilder.Entity<Expense>().HasData(
            new Expense
            {
                Id = 1,
                Title = "Grocery Shopping",
                Amount = 125.50m,
                Category = "Food & Dining",
                Description = "Weekly grocery shopping at supermarket",
                Date = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Expense
            {
                Id = 2,
                Title = "Gas Station",
                Amount = 45.00m,
                Category = "Transportation",
                Description = "Fuel for car",
                Date = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Expense
            {
                Id = 3,
                Title = "Electric Bill",
                Amount = 89.23m,
                Category = "Bills & Utilities",
                Description = "Monthly electricity bill",
                Date = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}