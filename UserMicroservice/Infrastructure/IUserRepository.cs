using UserMicroservice.Domain.Entities;

namespace UserMicroservice.Infrastructure;

public interface IUserRepository
{
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserEntity> CreateUserAsync(UserEntity user);
    Task<UserEntity> UpdateUserAsync(UserEntity user);
    Task DeleteUserAsync(UserEntity user);
    Task<List<UserEntity>> GetUsersByIdsAsync(List<Guid> userIds);
}