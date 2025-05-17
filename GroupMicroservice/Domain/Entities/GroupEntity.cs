namespace GroupMicroservice.Domain.Entities;

public class GroupEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    public List<Guid> MemberIds { get; set; } = [];
    public Dictionary<Guid, Guid> Users { get; set; } = new(); // donor -> recipient
}