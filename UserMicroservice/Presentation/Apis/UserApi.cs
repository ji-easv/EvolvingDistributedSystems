using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Application;
using UserMicroservice.Domain.DTOs;

namespace UserMicroservice.Presentation.Apis;

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
        [FromServices] IUserService userService,
        HttpContext httpContext,
        [FromBody] CreateUserDto createUserDto)
    {
        var apiVersion = httpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        var user = await userService.CreateUserAsync(createUserDto);
        return TypedResults.Created($"/api/v{apiVersion}/user/{user.Id}", user);
    }

    private static async Task<Results<Ok<GetUserDto>, ProblemHttpResult>> UpdateUserAsync(
        [FromServices] IUserService userService,
        [FromBody] UpdateUserDto updateUserDto)
    {
        var updatedUser = await userService.UpdateUserAsync(updateUserDto);
        return TypedResults.Ok(updatedUser);
    }

    private static async Task<Results<Ok, ProblemHttpResult>> DeleteUserAsync([FromServices] IUserService userService,
        Guid userId)
    {
        await userService.DeleteUserAsync(userId);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<GetUserDto>, ProblemHttpResult>> GetUserByIdAsync(
        [FromServices] IUserService userService, Guid userId)
    {
        var user = await userService.GetUserByIdAsync(userId);
        return TypedResults.Ok(user);
    }
}