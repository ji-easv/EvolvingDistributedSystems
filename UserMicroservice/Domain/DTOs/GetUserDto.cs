namespace UserMicroservice.Domain.DTOs;

public class GetUserDto
{
    public required Guid Id { get; set; }
    public required string Nickname { get; set; } 
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
}