using Asp.Versioning;
using GroupMicroservice.Application;
using GroupMicroservice.Domain.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GroupMicroservice.Presentation.Apis;

public static class GroupApi
{
    public static RouteGroupBuilder AddGroupApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("/api/v{version:apiVersion}/group")
            .WithTags("Group");

        api.MapPost("/", CreateGroupAsync)
            .WithName("CreateGroup");
        
        api.MapPut("/", UpdateGroupAsync)
            .WithName("UpdateGroup");
        
        api.MapDelete("/{groupId:guid}", DeleteGroupAsync)
            .WithName("DeleteGroup");
        
        api.MapGet("/{groupId:guid}", GetGroupByIdAsync)
            .WithName("GetGroupById");
        
        api.MapPatch("/{groupId:guid}/members", AddMemberToGroupAsync)
            .WithName("AddMemberToGroup");
        
        api.MapDelete("/{groupId:guid}/members/{userId:guid}", RemoveMemberFromGroupAsync)
            .WithName("RemoveMemberFromGroup");
        
        return api;
    }
    
    private static async Task<Results<Ok<GetGroupDto>, ProblemHttpResult>> UpdateGroupAsync(
        [FromServices] IGroupService groupService,
        [FromBody] UpdateGroupDto updateGroupDto)
    {
        var updatedGroup = await groupService.UpdateGroupAsync(updateGroupDto);
        return TypedResults.Ok(updatedGroup);
    }
    
    private static async Task<NoContent> DeleteGroupAsync([FromServices] IGroupService groupService,
        [FromRoute] Guid groupId)
    {
        var result = await groupService.DeleteGroupAsync(groupId);
        return TypedResults.NoContent();
    }
    
    private static async Task<Results<Ok<GetGroupDto>, ProblemHttpResult>> GetGroupByIdAsync(
        [FromServices] IGroupService groupService, [FromRoute] Guid groupId)
    {
        var group = await groupService.GetGroupByIdAsync(groupId);
        return TypedResults.Ok(group);
    }

    private static async Task<Results<Created<GetGroupDto>, ProblemHttpResult>> CreateGroupAsync(
        [FromServices] IGroupService groupService,
        HttpContext httpContext,
        [FromBody] CreateGroupDto createGroupDto)
    {
        var apiVersion = httpContext.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        var group = await groupService.CreateGroupAsync(createGroupDto);
        return TypedResults.Created($"/api/v{apiVersion}/group/{group.Id}", group);
    }
    
    private static async Task<NoContent> AddMemberToGroupAsync([FromRoute] Guid groupId, [FromQuery] Guid userId,
        [FromServices] IGroupService groupService)
    {
        var result = await groupService.AddMemberToGroupAsync(groupId, userId);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveMemberFromGroupAsync([FromServices] IGroupService groupService,
       [FromRoute] Guid groupId, [FromRoute] Guid userId)
    {
        var result = await groupService.RemoveMemberFromGroupAsync(groupId, userId);
        return TypedResults.NoContent();
    }
}