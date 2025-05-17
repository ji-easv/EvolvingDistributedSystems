using GroupMicroservice.Domain.DTOs;
using GroupMicroservice.Domain.Entities;

namespace GroupMicroservice.Domain;

public static class GroupMapper
{
    public static GetGroupDto ToGetGroupDto(this GroupEntity group, List<GetUserDto> users)
    {
        var groupDto = new GetGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Members = users,
            Pairs = group.Users
                .Select(pair =>
                {
                    var donor = users.FirstOrDefault(u => u.Id == pair.Key);
                    var recipient = users.FirstOrDefault(u => u.Id == pair.Value);

                    return (donor, recipient);
                })
                .Where(pair => pair.donor != null && pair.recipient != null)
                .ToDictionary(pair => pair.donor!, pair => pair.recipient!)
        };

        return groupDto;
    }
}