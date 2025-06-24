using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Api.DTOs;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId}/insights")]
[Authorize]
public class GroupInsightsController : ControllerBase
{
    private readonly IGroupInsightsService _groupInsightsService;

    public GroupInsightsController(IGroupInsightsService groupInsightsService)
    {
        _groupInsightsService = groupInsightsService;
    }

    [HttpGet]
    public async Task<ActionResult<GroupInsightsDto>> GetGroupInsights(int groupId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var insights = await _groupInsightsService.GetGroupInsightsAsync(groupId, userId);
            return Ok(insights);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving group insights: {ex.Message}");
        }
    }

    [HttpGet("members")]
    public async Task<ActionResult<MemberSpendingOverviewDto>> GetMemberSpendingOverview(int groupId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var memberSpending = await _groupInsightsService.GetMemberSpendingOverviewAsync(groupId, userId);
            return Ok(memberSpending);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving member spending: {ex.Message}");
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryInsightDto>>> GetCategoryInsights(int groupId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var categoryInsights = await _groupInsightsService.GetCategoryInsightsAsync(groupId, userId);
            return Ok(categoryInsights);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving category insights: {ex.Message}");
        }
    }

    [HttpGet("rankings")]
    public async Task<ActionResult<List<MemberRankingDto>>> GetMemberRankings(int groupId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var rankings = await _groupInsightsService.GetMemberRankingsAsync(groupId, userId);
            return Ok(rankings);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving member rankings: {ex.Message}");
        }
    }

    [HttpGet("trends")]
    public async Task<ActionResult<List<SpendingTrendDto>>> GetSpendingTrends(
        int groupId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Default to last 30 days if no dates provided
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            var trends = await _groupInsightsService.GetSpendingTrendsAsync(groupId, userId, start, end);
            return Ok(trends);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving spending trends: {ex.Message}");
        }
    }

    [HttpGet("timerange/{timeRange}")]
    public async Task<ActionResult<TimeRangeInsightsDto>> GetTimeRangeInsights(int groupId, string timeRange)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (!IsValidTimeRange(timeRange))
            {
                return BadRequest("Invalid time range. Supported values: week, month, all");
            }
            
            var insights = await _groupInsightsService.GetTimeRangeInsightsAsync(groupId, userId, timeRange);
            return Ok(insights);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You are not a member of this group");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving time range insights: {ex.Message}");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    private static bool IsValidTimeRange(string timeRange)
    {
        var validRanges = new[] { "week", "month", "all" };
        return validRanges.Contains(timeRange.ToLower());
    }
}