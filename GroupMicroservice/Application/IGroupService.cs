using GroupMicroservice.Domain.DTOs;

namespace GroupMicroservice.Application;

public interface IGroupService
{
    Task<GetGroupDto> GetGroupByIdAsync(Guid groupId);
    Task<GetGroupDto> CreateGroupAsync(CreateGroupDto group);
    Task<GetGroupDto> UpdateGroupAsync(UpdateGroupDto group);
    Task<bool> DeleteGroupAsync(Guid groupId);
    Task<List<GetUserDto>> GetGroupMembersAsync(Guid groupId);
    Task<bool> AddMemberToGroupAsync(Guid groupId, Guid userId);
    Task<bool> RemoveMemberFromGroupAsync(Guid groupId, Guid userId);
}