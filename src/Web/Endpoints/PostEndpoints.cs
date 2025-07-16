using Application.Contracts;
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
            .RequireAuthorization();

        group.MapGet("/{postID:guid}", GetPostById)
            .WithName("GetPostById")
            .Produces<PostDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllPosts)
            .WithName("GetFeed")
            .Produces<List<PostDto>>(StatusCodes.Status200OK);

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
        var result = await posts.CreatePostAsync(dto, currentUser.UserID, ct);
        if(result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        return Results.Created($"/api/v1/posts/{result.Value}", new { Id = result.Value });
    }

    private static async Task<IResult> GetPostById(Guid postID, [FromServices] IPostService posts, CancellationToken ct)
    {
        var post = await posts.GetPostAsync(postID, ct);
        if (post == null)
            return Results.NotFound();

        var postDto = new PostDto(post.ID, post.Author.Username.Value, post.Content, post.CreatedAt);
        return Results.Ok(postDto);
    }

    private static async Task<IResult> GetAllPosts([FromServices] IPostService posts, CancellationToken ct)
    {
        var allPosts = new List<Post>();
        await foreach (var post in posts.GetAllAsync(ct))
        {
            allPosts.Add(post);
        }
        
        var postDtos = new List<PostDto>();
        foreach (var post in allPosts)
        {
            postDtos.Add(new PostDto(post.ID, post.Author.Username.Value, post.Content, post.CreatedAt));
        }
        return Results.Ok(postDtos);
    }

    private static async Task<IResult> DeletePost(Guid postID, [FromServices] IPostService posts, [FromServices] ICurrentUserService currentUser, CancellationToken ct)
    {
        var post = await posts.GetPostAsync(postID, ct);
        if (post == null)
            return Results.NotFound();

        if (post.AuthorID != currentUser.UserID)
            return Results.Forbid();

        await posts.DeletePostAsync(post.ID, ct);
        
        return Results.NoContent();
    }
}