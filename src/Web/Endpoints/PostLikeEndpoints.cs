using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Web.Endpoints;

public static class PostLikeEndpoints
{
    public static IEndpointRouteBuilder MapPostLikeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts")
            .WithTags("Posts Likes");
        
        // Restful standard
        group.MapPost("/{postID:guid}/like", LikePost)
            .WithName("LikePost")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("/{postID:guid}/like", UnlikePost)
            .WithName("UnlikePost")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        
        return app;
    }
    
    
    private static async Task<IResult> LikePost(Guid postID, [FromServices] IPostLikeService posts, [FromServices] ICurrentUserService currentUser,
        CancellationToken ct)
    {
        /*var result = await posts.LikePostAsync(postID, currentUser.ID, ct);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));*/

        return Results.Ok();
    }
    
    private static async Task<IResult> UnlikePost(Guid postID, [FromServices] IPostLikeService posts, [FromServices] ICurrentUserService currentUser,
        CancellationToken ct)
    {
        /*var result = await posts.UnlikePostAsync(postID, currentUser.UserID, ct);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));*/

        return Results.NoContent();
    }
}