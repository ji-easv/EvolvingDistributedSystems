namespace GroupMicroservice.Domain.DTOs;

public class GetGroupDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    public List<GetUserDto> Members { get; set; } = [];
    public Dictionary<GetUserDto, GetUserDto> Pairs { get; set; } = new(); // donor -> recipient
}