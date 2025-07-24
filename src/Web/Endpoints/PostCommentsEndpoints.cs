using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Endpoints;

public static class PostCommentsEndpoints
{
    public static IEndpointRouteBuilder MapPostCommentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts/{postID:guid}/comments")
            .WithTags("Posts Comments");

        // Restful standard
        group.MapPost("/", CreateComment)
            .WithName("CreateComment")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .RequireAuthorization();

        group.MapGet("/", GetComments)
            .WithName("GetComments")
            .Produces<PaginatedResponse<CommentDto>>(StatusCodes.Status200OK);

        group.MapDelete("/{commentID:guid}", DeleteComment)
            .WithName("DeleteComment")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        
        return app;
    }
    
    private static async Task<IResult> CreateComment(Guid postID, [FromBody] CreateCommentDto dto, [FromServices] IPostCommentService posts, [FromServices] ICurrentUserService currentUser,
        CancellationToken ct)
    {
        /*var result = await posts.CreateCommentAsync(postID, currentUser.UserID, dto, ct);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        return Results.Created($"/api/v1/posts/{postID}/comments/{result}", new { Id = result });*/
        return null;
    }
    
    private static async Task<IResult> GetComments(Guid postID, [FromQuery] int page, [FromQuery] int pageSize, [FromServices] IPostCommentService posts, CancellationToken ct)
    {
        try
        {
            var paged = await posts.GetAllCommentsAsync(postID, page, pageSize, ct);
           
            return Results.Ok
            (
                new PaginatedResponse<CommentDto>
                (
                    Items: paged.Items,
                    Page: paged.Page,
                    PageSize: paged.PageSize,
                    TotalCount: paged.TotalCount
                )
            );

        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }
    
    private static async Task<IResult> DeleteComment(Guid postID, Guid commentID, [FromServices] IPostCommentService postComments, [FromServices] ICurrentUserService currentUser,
        CancellationToken ct)
    {
        /*var comment = await postComments.GetCommentAsync(commentID, ct);
        if(comment is null)
            return Results.NotFound();
        
        if (comment.AuthorID != currentUser.UserID)
            return Results.Forbid();
        
        var result = await postComments.DeleteCommentAsync(postID, comment, ct);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));*/

        return Results.NoContent();
    }
    
}