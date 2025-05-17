using UserMicroservice.Domain.DTOs;

namespace UserMicroservice.Application;

public interface IUserService
{
    Task<GetUserDto> GetUserByIdAsync(Guid id);
    Task<GetUserDto> GetUserByEmailAsync(string email);
    Task<GetUserDto> CreateUserAsync(CreateUserDto user);
    Task<GetUserDto> UpdateUserAsync(UpdateUserDto user);
    Task DeleteUserAsync(Guid userId);
    Task<List<GetUserDto>> GetUsersAsync(List<Guid> userIds);
}