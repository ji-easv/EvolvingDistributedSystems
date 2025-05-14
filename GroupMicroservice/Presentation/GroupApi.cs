namespace GroupMicroservice.Presentation;

public static class GroupApi
{
    public static RouteGroupBuilder AddGroupApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("/api/v{version:apiVersion}/group")
            .WithTags("Group");
        
        api.MapGet("hello", () => "Hello from Group API")
            .WithName("GetHello")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return api;
    }
}