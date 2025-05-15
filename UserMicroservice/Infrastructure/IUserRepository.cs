using UserMicroservice.Domain.Entities;

namespace UserMicroservice.Infrastructure;

public interface IUserRepository
{
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(UserEntity user);
    Task UpdateUserAsync(UserEntity user);
    Task DeleteUserAsync(UserEntity user);
}