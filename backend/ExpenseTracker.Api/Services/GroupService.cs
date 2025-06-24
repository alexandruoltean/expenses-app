using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public class GroupService : IGroupService
{
    private readonly ExpenseContext _context;
    private readonly ILogger<GroupService> _logger;

    public GroupService(ExpenseContext context, ILogger<GroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<GroupDto>> GetUserGroupsAsync(Guid userId)
    {
        try
        {
            var groups = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Include(ug => ug.Group)
                .ThenInclude(g => g.CreatedByUser)
                .Include(ug => ug.Group)
                .ThenInclude(g => g.UserGroups)
                .Select(ug => new GroupDto
                {
                    Id = ug.Group.Id,
                    Name = ug.Group.Name,
                    Description = ug.Group.Description,
                    InviteCode = ug.Group.InviteCode,
                    CreatedBy = ug.Group.CreatedBy,
                    CreatedByUsername = ug.Group.CreatedByUser.Username,
                    CreatedAt = ug.Group.CreatedAt,
                    MemberCount = ug.Group.UserGroups.Count,
                    UserRole = ug.Role
                })
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups for user {UserId}", userId);
            throw;
        }
    }

    public async Task<GroupDto?> GetGroupAsync(int groupId, Guid userId)
    {
        try
        {
            var userGroup = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId && ug.UserId == userId)
                .Include(ug => ug.Group)
                .ThenInclude(g => g.CreatedByUser)
                .Include(ug => ug.Group)
                .ThenInclude(g => g.UserGroups)
                .FirstOrDefaultAsync();

            if (userGroup == null)
                return null;

            return new GroupDto
            {
                Id = userGroup.Group.Id,
                Name = userGroup.Group.Name,
                Description = userGroup.Group.Description,
                InviteCode = userGroup.Group.InviteCode,
                CreatedBy = userGroup.Group.CreatedBy,
                CreatedByUsername = userGroup.Group.CreatedByUser.Username,
                CreatedAt = userGroup.Group.CreatedAt,
                MemberCount = userGroup.Group.UserGroups.Count,
                UserRole = userGroup.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId} for user {UserId}", groupId, userId);
            throw;
        }
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, Guid userId)
    {
        try
        {
            var inviteCode = GenerateInviteCode();
            
            // Ensure invite code is unique
            while (await _context.Groups.AnyAsync(g => g.InviteCode == inviteCode))
            {
                inviteCode = GenerateInviteCode();
            }

            var group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                InviteCode = inviteCode,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add creator as admin member
            var userGroup = new UserGroup
            {
                UserId = userId,
                GroupId = group.Id,
                Role = "Admin",
                JoinedAt = DateTime.UtcNow
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            // Load the creator info
            var creator = await _context.Users.FindAsync(userId);

            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                InviteCode = group.InviteCode,
                CreatedBy = group.CreatedBy,
                CreatedByUsername = creator?.Username ?? "",
                CreatedAt = group.CreatedAt,
                MemberCount = 1,
                UserRole = "Admin"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group for user {UserId}", userId);
            throw;
        }
    }

    public async Task<GroupDto?> JoinGroupAsync(string inviteCode, Guid userId)
    {
        try
        {
            var group = await _context.Groups
                .Include(g => g.CreatedByUser)
                .Include(g => g.UserGroups)
                .FirstOrDefaultAsync(g => g.InviteCode == inviteCode);

            if (group == null)
                return null;

            // Check if user is already in group
            var existingMembership = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == group.Id);

            if (existingMembership != null)
                return null; // User already in group

            var userGroup = new UserGroup
            {
                UserId = userId,
                GroupId = group.Id,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                InviteCode = group.InviteCode,
                CreatedBy = group.CreatedBy,
                CreatedByUsername = group.CreatedByUser.Username,
                CreatedAt = group.CreatedAt,
                MemberCount = group.UserGroups.Count + 1,
                UserRole = "Member"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group with invite code {InviteCode} for user {UserId}", inviteCode, userId);
            throw;
        }
    }

    public async Task<bool> LeaveGroupAsync(int groupId, Guid userId)
    {
        try
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup == null)
                return false;

            _context.UserGroups.Remove(userGroup);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId} for user {UserId}", groupId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(int groupId, Guid userId)
    {
        try
        {
            // First check if user is in the group
            var isUserInGroup = await IsUserInGroupAsync(userId, groupId);
            if (!isUserInGroup)
                return new List<GroupMemberDto>();

            var members = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Include(ug => ug.User)
                .Select(ug => new GroupMemberDto
                {
                    UserId = ug.UserId,
                    Username = ug.User.Username,
                    Email = ug.User.Email,
                    Role = ug.Role,
                    JoinedAt = ug.JoinedAt
                })
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

            return members;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for group {GroupId}", groupId);
            throw;
        }
    }

    public async Task<bool> IsUserInGroupAsync(Guid userId, int groupId)
    {
        try
        {
            return await _context.UserGroups
                .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is in group {GroupId}", userId, groupId);
            throw;
        }
    }

    public string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
}