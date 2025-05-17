using GroupMicroservice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GroupMicroservice.Infrastructure;

public class GroupRepository(AppDbContext dbContext) : IGroupRepository
{
    public async Task<GroupEntity?> GetGroupByIdAsync(Guid groupId)
    {
        var groupEntity = await dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        return groupEntity;
    }

    public async Task<GroupEntity> CreateGroupAsync(GroupEntity group)
    {
        var result = await dbContext.Groups.AddAsync(group);
        await dbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<GroupEntity> UpdateGroupAsync(GroupEntity group)
    {
        var result = dbContext.Groups.Update(group);
        await dbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<bool> DeleteGroupAsync(GroupEntity group)
    {
        var result = dbContext.Groups.Remove(group);
        await dbContext.SaveChangesAsync();
        return result.State == EntityState.Deleted;
    }
}