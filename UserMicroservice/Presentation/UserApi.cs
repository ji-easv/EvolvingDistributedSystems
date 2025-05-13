using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Domain.DTOs;
using UserMicroservice.Domain.Entities;
using UserMicroservice.Infrastructure;

namespace UserMicroservice.Presentation;

public static class UserApi
{
    public static RouteGroupBuilder AddUserApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("/api/v{version:apiVersion}/user")
            .WithTags("User");

        api.MapPost("/", CreateUserAsync)
            .WithName("CreateUser");

        api.MapPut("/", UpdateUserAsync)
            .WithName("UpdateUser");

        api.MapDelete("/{userId:guid}", DeleteUserAsync)
            .WithName("DeleteUser");

        api.MapGet("/{userId:guid}", GetUserByIdAsync)
            .WithName("GetUserById");

        return api;
    }

    private static async Task<Results<Created<GetUserDto>, ProblemHttpResult>> CreateUserAsync(
        [FromServices] UserRepository userRepository,
        HttpContext httpContext,
        [FromBody] CreateUserDto createUserDto)
    {
        var apiVersion = httpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Nickname = createUserDto.Nickname,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            CreatedAt = DateTime.UtcNow
        };
        
        await userRepository.CreateUserAsync(user);
        return TypedResults.Created($"/api/v{apiVersion}/user/{user.Id}", ToGetUserDto(user));
    }

    private static async Task<Results<Ok<GetUserDto>, ProblemHttpResult>> UpdateUserAsync([FromServices] UserRepository userRepository,
        [FromBody] UpdateUserDto updateUserDto)
    {
        var existingUser = await userRepository.GetUserByIdAsync(updateUserDto.Id);
        if (existingUser == null)
        {
            throw new Exception($"User with ID {updateUserDto.Id} not found.");
        }

        existingUser.Nickname = updateUserDto.Nickname;
        existingUser.Email = updateUserDto.Email;
        existingUser.Password = updateUserDto.Password;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateUserAsync(existingUser);
        return TypedResults.Ok(ToGetUserDto(existingUser));
    }

    private static async Task DeleteUserAsync([FromServices] UserRepository userRepository, Guid userId)
    {
        var existingUser = await userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }

        await userRepository.DeleteUserAsync(existingUser);
    }

    private static async Task<Results<Ok<GetUserDto>, ProblemHttpResult>> GetUserByIdAsync([FromServices] UserRepository userRepository, Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found.");
        }
        
        return TypedResults.Ok(ToGetUserDto(user));
    }

    private static GetUserDto ToGetUserDto(UserEntity user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            // CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}