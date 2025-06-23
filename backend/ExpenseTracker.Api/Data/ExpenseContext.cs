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

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        // Create test user with hashed password "password"
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password", salt);
        
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = testUserId,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            LastLoginAt = null
        });

        // Create sample expenses for the test user
        modelBuilder.Entity<Expense>().HasData(
            new Expense
            {
                Id = 1,
                Title = "Grocery Shopping",
                Amount = 125.50m,
                Category = "Food & Dining",
                Description = "Weekly groceries from supermarket",
                Date = DateTime.Now.AddDays(-2),
                CreatedAt = DateTime.Now.AddDays(-2),
                UpdatedAt = DateTime.Now.AddDays(-2),
                UserId = testUserId
            },
            new Expense
            {
                Id = 2,
                Title = "Gas Station",
                Amount = 45.75m,
                Category = "Transportation",
                Description = "Fill up car tank",
                Date = DateTime.Now.AddDays(-1),
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1),
                UserId = testUserId
            },
            new Expense
            {
                Id = 3,
                Title = "Coffee Shop",
                Amount = 8.50m,
                Category = "Food & Dining",
                Description = "Morning coffee and pastry",
                Date = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = testUserId
            },
            new Expense
            {
                Id = 4,
                Title = "Movie Tickets",
                Amount = 24.00m,
                Category = "Entertainment",
                Description = "Evening movie with friends",
                Date = DateTime.Now.AddDays(-3),
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = DateTime.Now.AddDays(-3),
                UserId = testUserId
            },
            new Expense
            {
                Id = 5,
                Title = "Electricity Bill",
                Amount = 89.25m,
                Category = "Bills & Utilities",
                Description = "Monthly electricity payment",
                Date = DateTime.Now.AddDays(-5),
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now.AddDays(-5),
                UserId = testUserId
            }
        );
    }
}