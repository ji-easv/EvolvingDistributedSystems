namespace MatchMicroservice.Presentation;

public static class GroupApi
{
    public static RouteGroupBuilder AddGroupApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("/api/v1/")
            .WithTags("Listing");
        
        
        return api;
    }
}