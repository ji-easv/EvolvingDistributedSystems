using UserMicroservice.Domain.DTOs;
using UserMicroservice.Domain.Entities;

namespace UserMicroservice.Domain;

public static class UserMapper
{
    public static GetUserDto ToGetUserDto(this UserEntity user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}