using Microsoft.EntityFrameworkCore;
using UserMicroservice.Domain.Entities;

namespace UserMicroservice.Infrastructure;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity> CreateUserAsync(UserEntity user)
    {
        var result = await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<UserEntity> UpdateUserAsync(UserEntity user)
    {
        var result = context.Users.Update(user);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteUserAsync(UserEntity user)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}