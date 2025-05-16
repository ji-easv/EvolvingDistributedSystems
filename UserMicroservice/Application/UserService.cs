using UserMicroservice.Domain;
using UserMicroservice.Domain.DTOs;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Domain.Exceptions;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Application;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<GetUserDto> GetUserByIdAsync(Guid id)
    {
        var user = await GetUserEntityByIdAsync(id);
        return user.ToGetUserDto();
    }

    public async Task<GetUserDto> GetUserByEmailAsync(string email)
    {
        var user = await userRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException($"User with email {email} not found.");
        }

        return user.ToGetUserDto();
    }

    public async Task<GetUserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = await userRepository.CreateUserAsync(new UserEntity
        {
            Id = Guid.NewGuid(),
            Nickname = createUserDto.Nickname,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            CreatedAt = DateTime.UtcNow
        });
    
        return user.ToGetUserDto();
    }

    public async Task<GetUserDto> UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        var existingUser = await GetUserEntityByIdAsync(updateUserDto.Id); 
        existingUser.Nickname = updateUserDto.Nickname;
        existingUser.Email = updateUserDto.Email;
        existingUser.UpdatedAt = DateTime.UtcNow;
        
        var updatedUser = await userRepository.UpdateUserAsync(existingUser);
        return updatedUser.ToGetUserDto();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var existingUser = await GetUserEntityByIdAsync(userId);
        await userRepository.DeleteUserAsync(existingUser);
    }

    private async Task<UserEntity> GetUserEntityByIdAsync(Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found.");
        }
        
        return user;
    }
}