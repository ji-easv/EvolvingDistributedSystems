namespace GroupMicroservice.Domain.DTOs;

public class CreateGroupDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Guid OwnerId { get; set; }
}