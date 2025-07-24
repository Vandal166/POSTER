using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/posts")
            .WithTags("Posts");

        group.MapPost("/", CreatePost)
            .WithName("CreatePost")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .RequireAuthorization("CanCreatePost");

        group.MapGet("/{postID:guid}", GetPostById)
            .WithName("GetPostById")
            .Produces<PostDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllPosts)
            .WithName("GetFeed")
            .Produces<PaginatedResponse<PostDto>>(StatusCodes.Status200OK);

        group.MapDelete("/{postID:guid}", DeletePost)
            .WithName("DeletePost")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        
        return app;
    }
    
    private static async Task<IResult> CreatePost([FromBody] CreatePostDto dto, [FromServices] IPostService posts, [FromServices] ICurrentUserService currentUser,
        CancellationToken ct)
    {
        /*var result = await posts.CreatePostAsync(dto, currentUser.UserID, ct);
        if(result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        return Results.Created($"/api/v1/posts/{result.Value}", new { Id = result.Value });*/
        return null;
    }

    private static async Task<IResult> GetPostById(Guid postID, [FromServices] IPostService posts, CancellationToken ct)
    {
        /*var post = await posts.GetPostAsync(postID, ct);
        if (post is null)
            return Results.NotFound();

        var postDto = new PostDto(post.ID, post.Author.Username.Value, post.Content, post.CreatedAt);
        return Results.Ok(postDto);*/ return null;
    }

    private static async Task<IResult> GetAllPosts([FromQuery] int page, [FromQuery] int pageSize, [FromServices] IPostService posts, CancellationToken ct)
    {
        var paged = await posts.GetAllAsync(page, pageSize, ct);
        return Results.Ok
        (
            new PaginatedResponse<PostDto>
            (
                Items: paged.Items,
                Page: paged.Page,
                PageSize: paged.PageSize,
                TotalCount: paged.TotalCount
            )
        );
    }

    private static async Task<IResult> DeletePost(Guid postID, [FromServices] IPostService posts, [FromServices] ICurrentUserService currentUser, CancellationToken ct)
    {
        /*var post = await posts.GetPostAsync(postID, ct);
        if (post is null)
            return Results.NotFound();

        if (post.AuthorID != currentUser.UserID)
            return Results.Forbid();

        await posts.DeletePostAsync(post.ID, ct);*/
        
        return Results.NoContent();
    }
}