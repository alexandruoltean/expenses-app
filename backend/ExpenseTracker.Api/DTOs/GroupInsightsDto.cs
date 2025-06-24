using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.DTOs;

public class GroupInsightsDto
{
    public MemberSpendingOverviewDto MemberSpending { get; set; } = new();
    public List<CategoryInsightDto> CategoryInsights { get; set; } = new();
    public List<MemberRankingDto> Rankings { get; set; } = new();
    public GroupStatisticsDto Statistics { get; set; } = new();
}

public class MemberSpendingOverviewDto
{
    public decimal TotalGroupSpending { get; set; }
    public List<MemberSpendingDto> Members { get; set; } = new();
}

public class MemberSpendingDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public int ExpenseCount { get; set; }
    public decimal AverageExpense { get; set; }
    public List<CategorySpendingDto> CategoryBreakdown { get; set; } = new();
}

public class CategorySpendingDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class CategoryInsightDto
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<MemberCategorySpendingDto> MemberBreakdown { get; set; } = new();
    public string TopSpender { get; set; } = string.Empty;
    public decimal TopSpenderAmount { get; set; }
}

public class MemberCategorySpendingDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}

public class MemberRankingDto
{
    public string Category { get; set; } = string.Empty;
    public List<RankingEntryDto> Rankings { get; set; } = new();
}

public class RankingEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class GroupStatisticsDto
{
    public decimal AverageExpensePerMember { get; set; }
    public decimal MedianExpenseAmount { get; set; }
    public int TotalExpenses { get; set; }
    public int ActiveMembers { get; set; }
    public DateTime? FirstExpenseDate { get; set; }
    public DateTime? LastExpenseDate { get; set; }
    public string MostActiveCategory { get; set; } = string.Empty;
    public string MostExpensiveCategory { get; set; } = string.Empty;
}

public class SpendingTrendDto
{
    public DateTime Date { get; set; }
    public List<MemberDailySpendingDto> MemberSpending { get; set; } = new();
    public decimal TotalDayAmount { get; set; }
}

public class MemberDailySpendingDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int ExpenseCount { get; set; }
}

public class TimeRangeInsightsDto
{
    public string TimeRange { get; set; } = string.Empty; // "week", "month", "all"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MemberSpendingOverviewDto MemberSpending { get; set; } = new();
    public List<SpendingTrendDto> Trends { get; set; } = new();
    public GroupStatisticsDto Statistics { get; set; } = new();
}