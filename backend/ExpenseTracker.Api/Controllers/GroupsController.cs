using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetUserGroups()
    {
        try
        {
            var userId = GetCurrentUserId();
            var groups = await _groupService.GetUserGroupsAsync(userId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups");
            return StatusCode(500, "An error occurred while getting groups");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.GetGroupAsync(id, userId);
            
            if (group == null)
                return NotFound("Group not found or access denied");

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return StatusCode(500, "An error occurred while getting the group");
        }
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Group name is required");

            var userId = GetCurrentUserId();
            var group = await _groupService.CreateGroupAsync(request, userId);
            
            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return StatusCode(500, "An error occurred while creating the group");
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult<GroupDto>> JoinGroup([FromBody] JoinGroupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.InviteCode))
                return BadRequest("Invite code is required");

            var userId = GetCurrentUserId();
            var group = await _groupService.JoinGroupAsync(request.InviteCode, userId);
            
            if (group == null)
                return BadRequest("Invalid invite code or already a member");

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group with invite code {InviteCode}", request.InviteCode);
            return StatusCode(500, "An error occurred while joining the group");
        }
    }

    [HttpDelete("{id}/leave")]
    public async Task<ActionResult> LeaveGroup(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _groupService.LeaveGroupAsync(id, userId);
            
            if (!success)
                return NotFound("Group not found or not a member");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId}", id);
            return StatusCode(500, "An error occurred while leaving the group");
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetGroupMembers(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var members = await _groupService.GetGroupMembersAsync(id, userId);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for group {GroupId}", id);
            return StatusCode(500, "An error occurred while getting group members");
        }
    }
}