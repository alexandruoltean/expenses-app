using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public class GroupInsightsService : IGroupInsightsService
{
    private readonly ExpenseContext _context;
    private readonly IGroupService _groupService;

    public GroupInsightsService(ExpenseContext context, IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    public async Task<GroupInsightsDto> GetGroupInsightsAsync(int groupId, Guid userId)
    {
        // Verify user has access to the group
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var memberSpending = await GetMemberSpendingOverviewAsync(groupId, userId);
        var categoryInsights = await GetCategoryInsightsAsync(groupId, userId);
        var rankings = await GetMemberRankingsAsync(groupId, userId);
        var statistics = await GetGroupStatisticsAsync(groupId);

        return new GroupInsightsDto
        {
            MemberSpending = memberSpending,
            CategoryInsights = categoryInsights,
            Rankings = rankings,
            Statistics = statistics
        };
    }

    public async Task<MemberSpendingOverviewDto> GetMemberSpendingOverviewAsync(int groupId, Guid userId)
    {
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var groupExpenses = await _context.Expenses
            .Include(e => e.User)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();

        var totalGroupSpending = groupExpenses.Sum(e => e.Amount);

        var memberSpending = groupExpenses
            .GroupBy(e => e.User)
            .Select(g => new MemberSpendingDto
            {
                UserId = g.Key.Id,
                Username = g.Key.Username,
                Email = g.Key.Email,
                TotalAmount = g.Sum(e => e.Amount),
                Percentage = totalGroupSpending > 0 ? (g.Sum(e => e.Amount) / totalGroupSpending) * 100 : 0,
                ExpenseCount = g.Count(),
                AverageExpense = g.Count() > 0 ? g.Sum(e => e.Amount) / g.Count() : 0,
                CategoryBreakdown = g.GroupBy(e => e.Category)
                    .Select(cg => new CategorySpendingDto
                    {
                        Category = cg.Key,
                        Amount = cg.Sum(e => e.Amount),
                        Count = cg.Count(),
                        Percentage = g.Sum(e => e.Amount) > 0 ? (cg.Sum(e => e.Amount) / g.Sum(e => e.Amount)) * 100 : 0
                    })
                    .OrderByDescending(c => c.Amount)
                    .ToList()
            })
            .OrderByDescending(m => m.TotalAmount)
            .ToList();

        return new MemberSpendingOverviewDto
        {
            TotalGroupSpending = totalGroupSpending,
            Members = memberSpending
        };
    }

    public async Task<List<CategoryInsightDto>> GetCategoryInsightsAsync(int groupId, Guid userId)
    {
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var groupExpenses = await _context.Expenses
            .Include(e => e.User)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();

        var categoryInsights = groupExpenses
            .GroupBy(e => e.Category)
            .Select(cg =>
            {
                var memberBreakdown = cg
                    .GroupBy(e => e.User)
                    .Select(ug => new MemberCategorySpendingDto
                    {
                        UserId = ug.Key.Id,
                        Username = ug.Key.Username,
                        Amount = ug.Sum(e => e.Amount),
                        Percentage = cg.Sum(e => e.Amount) > 0 ? (ug.Sum(e => e.Amount) / cg.Sum(e => e.Amount)) * 100 : 0,
                        Count = ug.Count()
                    })
                    .OrderByDescending(m => m.Amount)
                    .ToList();

                var topSpender = memberBreakdown.FirstOrDefault();

                return new CategoryInsightDto
                {
                    Category = cg.Key,
                    TotalAmount = cg.Sum(e => e.Amount),
                    MemberBreakdown = memberBreakdown,
                    TopSpender = topSpender?.Username ?? "",
                    TopSpenderAmount = topSpender?.Amount ?? 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return categoryInsights;
    }

    public async Task<List<MemberRankingDto>> GetMemberRankingsAsync(int groupId, Guid userId)
    {
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var memberSpending = await GetMemberSpendingOverviewAsync(groupId, userId);
        var rankings = new List<MemberRankingDto>();

        // Total spending ranking
        rankings.Add(new MemberRankingDto
        {
            Category = "Total Spending",
            Rankings = memberSpending.Members
                .Select((m, index) => new RankingEntryDto
                {
                    Rank = index + 1,
                    UserId = m.UserId,
                    Username = m.Username,
                    Value = m.TotalAmount,
                    AdditionalInfo = $"{m.ExpenseCount} expenses"
                })
                .ToList()
        });

        // Average expense ranking
        rankings.Add(new MemberRankingDto
        {
            Category = "Average Expense",
            Rankings = memberSpending.Members
                .OrderByDescending(m => m.AverageExpense)
                .Select((m, index) => new RankingEntryDto
                {
                    Rank = index + 1,
                    UserId = m.UserId,
                    Username = m.Username,
                    Value = m.AverageExpense,
                    AdditionalInfo = $"{m.ExpenseCount} expenses"
                })
                .ToList()
        });

        // Most active (expense count) ranking
        rankings.Add(new MemberRankingDto
        {
            Category = "Most Active",
            Rankings = memberSpending.Members
                .OrderByDescending(m => m.ExpenseCount)
                .Select((m, index) => new RankingEntryDto
                {
                    Rank = index + 1,
                    UserId = m.UserId,
                    Username = m.Username,
                    Value = m.ExpenseCount,
                    AdditionalInfo = $"${m.TotalAmount:F2} total"
                })
                .ToList()
        });

        return rankings;
    }

    public async Task<TimeRangeInsightsDto> GetTimeRangeInsightsAsync(int groupId, Guid userId, string timeRange)
    {
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var (startDate, endDate) = GetTimeRangeDates(timeRange);

        var groupExpenses = await _context.Expenses
            .Include(e => e.User)
            .Where(e => e.GroupId == groupId && e.Date >= startDate && e.Date <= endDate)
            .ToListAsync();

        var totalGroupSpending = groupExpenses.Sum(e => e.Amount);

        var memberSpending = groupExpenses
            .GroupBy(e => e.User)
            .Select(g => new MemberSpendingDto
            {
                UserId = g.Key.Id,
                Username = g.Key.Username,
                Email = g.Key.Email,
                TotalAmount = g.Sum(e => e.Amount),
                Percentage = totalGroupSpending > 0 ? (g.Sum(e => e.Amount) / totalGroupSpending) * 100 : 0,
                ExpenseCount = g.Count(),
                AverageExpense = g.Count() > 0 ? g.Sum(e => e.Amount) / g.Count() : 0
            })
            .OrderByDescending(m => m.TotalAmount)
            .ToList();

        var trends = await GetSpendingTrendsAsync(groupId, userId, startDate, endDate);
        var statistics = await GetGroupStatisticsAsync(groupId, startDate, endDate);

        return new TimeRangeInsightsDto
        {
            TimeRange = timeRange,
            StartDate = startDate,
            EndDate = endDate,
            MemberSpending = new MemberSpendingOverviewDto
            {
                TotalGroupSpending = totalGroupSpending,
                Members = memberSpending
            },
            Trends = trends,
            Statistics = statistics
        };
    }

    public async Task<List<SpendingTrendDto>> GetSpendingTrendsAsync(int groupId, Guid userId, DateTime startDate, DateTime endDate)
    {
        if (!await _groupService.IsUserInGroupAsync(userId, groupId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var groupExpenses = await _context.Expenses
            .Include(e => e.User)
            .Where(e => e.GroupId == groupId && e.Date >= startDate && e.Date <= endDate)
            .ToListAsync();

        var trends = new List<SpendingTrendDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            var dayExpenses = groupExpenses.Where(e => e.Date.Date == currentDate).ToList();
            
            var memberDailySpending = dayExpenses
                .GroupBy(e => e.User)
                .Select(g => new MemberDailySpendingDto
                {
                    UserId = g.Key.Id,
                    Username = g.Key.Username,
                    Amount = g.Sum(e => e.Amount),
                    ExpenseCount = g.Count()
                })
                .ToList();

            trends.Add(new SpendingTrendDto
            {
                Date = currentDate,
                MemberSpending = memberDailySpending,
                TotalDayAmount = dayExpenses.Sum(e => e.Amount)
            });

            currentDate = currentDate.AddDays(1);
        }

        return trends;
    }

    private async Task<GroupStatisticsDto> GetGroupStatisticsAsync(int groupId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Expenses
            .Include(e => e.User)
            .Where(e => e.GroupId == groupId);

        if (startDate.HasValue)
            query = query.Where(e => e.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(e => e.Date <= endDate.Value);

        var expenses = await query.ToListAsync();

        if (!expenses.Any())
        {
            return new GroupStatisticsDto
            {
                ActiveMembers = 0,
                TotalExpenses = 0
            };
        }

        var expenseAmounts = expenses.Select(e => e.Amount).OrderBy(a => a).ToList();
        var median = expenseAmounts.Count % 2 == 0
            ? (expenseAmounts[expenseAmounts.Count / 2 - 1] + expenseAmounts[expenseAmounts.Count / 2]) / 2
            : expenseAmounts[expenseAmounts.Count / 2];

        var categoryTotals = expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var categoryCounts = expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        return new GroupStatisticsDto
        {
            AverageExpensePerMember = expenses.GroupBy(e => e.UserId).Average(g => g.Sum(e => e.Amount)),
            MedianExpenseAmount = median,
            TotalExpenses = expenses.Count,
            ActiveMembers = expenses.GroupBy(e => e.UserId).Count(),
            FirstExpenseDate = expenses.Min(e => e.Date),
            LastExpenseDate = expenses.Max(e => e.Date),
            MostActiveCategory = categoryCounts.OrderByDescending(kvp => kvp.Value).First().Key,
            MostExpensiveCategory = categoryTotals.OrderByDescending(kvp => kvp.Value).First().Key
        };
    }

    private static (DateTime startDate, DateTime endDate) GetTimeRangeDates(string timeRange)
    {
        var now = DateTime.UtcNow.Date;
        
        return timeRange.ToLower() switch
        {
            "week" => (now.AddDays(-(int)now.DayOfWeek), now.AddDays(6 - (int)now.DayOfWeek)),
            "month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))),
            "all" => (DateTime.MinValue, DateTime.MaxValue),
            _ => throw new ArgumentException($"Invalid time range: {timeRange}")
        };
    }
}