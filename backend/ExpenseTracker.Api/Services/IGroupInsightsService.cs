using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IGroupInsightsService
{
    Task<GroupInsightsDto> GetGroupInsightsAsync(int groupId, Guid userId);
    Task<MemberSpendingOverviewDto> GetMemberSpendingOverviewAsync(int groupId, Guid userId);
    Task<List<CategoryInsightDto>> GetCategoryInsightsAsync(int groupId, Guid userId);
    Task<List<MemberRankingDto>> GetMemberRankingsAsync(int groupId, Guid userId);
    Task<TimeRangeInsightsDto> GetTimeRangeInsightsAsync(int groupId, Guid userId, string timeRange);
    Task<List<SpendingTrendDto>> GetSpendingTrendsAsync(int groupId, Guid userId, DateTime startDate, DateTime endDate);
}