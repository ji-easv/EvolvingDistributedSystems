namespace GroupMicroservice.Domain.DTOs;

public class CreateUserDto
{
    public required string Nickname { get; set; }
    public required string Email { get; set; } 
    public required string Password { get; set; }
}