namespace UserMicroservice.Domain.Entities;

public class UserEntity
{
    public required Guid Id { get; set; }
    public required string Nickname { get; set; }
    public required string Email { get; set; } 
    public required string Password { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}