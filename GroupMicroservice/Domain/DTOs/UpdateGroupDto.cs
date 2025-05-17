namespace GroupMicroservice.Domain.DTOs;

public class UpdateGroupDto
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}