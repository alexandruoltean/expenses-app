using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public interface IGroupService
{
    Task<IEnumerable<GroupDto>> GetUserGroupsAsync(Guid userId);
    Task<GroupDto?> GetGroupAsync(int groupId, Guid userId);
    Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, Guid userId);
    Task<GroupDto?> JoinGroupAsync(string inviteCode, Guid userId);
    Task<bool> LeaveGroupAsync(int groupId, Guid userId);
    Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(int groupId, Guid userId);
    Task<bool> IsUserInGroupAsync(Guid userId, int groupId);
    string GenerateInviteCode();
}