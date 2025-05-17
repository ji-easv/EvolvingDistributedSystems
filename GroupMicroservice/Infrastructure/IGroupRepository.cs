using GroupMicroservice.Domain.DTOs;
using GroupMicroservice.Domain.Entities;

namespace GroupMicroservice.Infrastructure;

public interface IGroupRepository
{
    Task<GroupEntity?> GetGroupByIdAsync(Guid groupId);
    Task<GroupEntity> CreateGroupAsync(GroupEntity group);
    Task<GroupEntity> UpdateGroupAsync(GroupEntity group);
    Task<bool> DeleteGroupAsync(GroupEntity group);
}