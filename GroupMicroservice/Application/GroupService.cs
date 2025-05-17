using GroupMicroservice.Domain;
using GroupMicroservice.Domain.DTOs;
using GroupMicroservice.Domain.Entities;
using GroupMicroservice.Domain.Exceptions;
using GroupMicroservice.Infrastructure;

namespace GroupMicroservice.Application;

public class GroupService(IGroupRepository groupRepository, UserApiClient userApiClient) : IGroupService
{
    public async Task<GetGroupDto> GetGroupByIdAsync(Guid groupId)
    {
        var group = await groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupId} not found.");
        }

        var users = await userApiClient.GetUsersByIdsAsync(group.MemberIds);
        return group.ToGetGroupDto(users);
    }

    public async Task<GetGroupDto> CreateGroupAsync(CreateGroupDto group)
    {
        var groupEntity = new GroupEntity
        {
            Id = Guid.NewGuid(),
            Name = group.Name,
            Description = group.Description,
        };

        var createdGroup = await groupRepository.CreateGroupAsync(groupEntity);
        return createdGroup.ToGetGroupDto([]);
    }

    public async Task<GetGroupDto> UpdateGroupAsync(UpdateGroupDto group)
    {
        var groupEntity = await groupRepository.GetGroupByIdAsync(group.Id);
        if (groupEntity == null)
        {
            throw new NotFoundException($"Group with ID {group.Id} not found.");
        }

        groupEntity.Name = group.Name ?? groupEntity.Name;
        groupEntity.Description = group.Description;

        var updatedGroup = await groupRepository.UpdateGroupAsync(groupEntity);
        return updatedGroup.ToGetGroupDto([]);
    }

    public async Task<bool> DeleteGroupAsync(Guid groupId)
    {
        var group = await groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupId} not found.");
        }

        return await groupRepository.DeleteGroupAsync(group);
    }

    public async Task<List<GetUserDto>> GetGroupMembersAsync(Guid groupId)
    {
        var group = await groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupId} not found.");
        }

        var users = await userApiClient.GetUsersByIdsAsync(group.MemberIds);
        return users;
    }

    public async Task<bool> AddMemberToGroupAsync(Guid groupId, Guid userId)
    {
        var group = await groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupId} not found.");
        }

        group.MemberIds.Add(userId);
        await groupRepository.UpdateGroupAsync(group);
        return true;
    }

    public async Task<bool> RemoveMemberFromGroupAsync(Guid groupId, Guid userId)
    {
        var group = await groupRepository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group with ID {groupId} not found.");
        }

        group.MemberIds.Remove(userId);
        await groupRepository.UpdateGroupAsync(group);
        return true;
    }
}