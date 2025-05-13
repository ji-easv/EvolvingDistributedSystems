namespace UserMicroservice.Domain.DTOs;

public class UpdateUserDto : CreateUserDto
{
    public required Guid Id { get; set; }
}