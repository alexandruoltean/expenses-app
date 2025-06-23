using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Data;

public class ExpenseContext : DbContext
{
    public ExpenseContext(DbContextOptions<ExpenseContext> options) : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.HasIndex(e => e.Email)
                .IsUnique();
                
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.PasswordHash)
                .IsRequired();
                
            entity.Property(e => e.Salt)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");
        });

        // Configure Expense entity
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
                
            // Configure relationship
            entity.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Note: Seed data removed temporarily until we implement user authentication
    }
}