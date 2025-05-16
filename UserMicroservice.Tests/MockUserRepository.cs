using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Tests;

public class MockUserRepository : IUserRepository
{
    private readonly List<UserEntity> _users = [];

    public Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task<UserEntity> CreateUserAsync(UserEntity user)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<UserEntity> UpdateUserAsync(UserEntity user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            existingUser.Email = user.Email;
            existingUser.Nickname = user.Nickname;
            existingUser.Password = user.Password;
            existingUser.CreatedAt = user.CreatedAt;
            existingUser.UpdatedAt = user.UpdatedAt;
        }
        return Task.FromResult(existingUser);
    }

    public Task DeleteUserAsync(UserEntity user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            _users.Remove(existingUser);
        }
        return Task.CompletedTask;
    }
}