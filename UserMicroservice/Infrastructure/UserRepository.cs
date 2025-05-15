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

    public async Task CreateUserAsync(UserEntity user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UserEntity user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(UserEntity user)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}