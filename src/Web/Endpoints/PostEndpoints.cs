using Application.Contracts;
using Application.Services;
using Domain.Entities;
using FluentValidation;
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

        group.MapGet("/{id:guid}", GetPostById)
            .WithName("GetPostById")
            .Produces<PostDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetFeed)
            .WithName("GetFeed")
            .Produces<List<PostDto>>(StatusCodes.Status200OK);

        // you can add update and delete:
        group.MapDelete("/{id:guid}", DeletePost)
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

    private static async Task<IResult> GetPostById(
        Guid id,
        [FromServices] IPostRepository posts,
        CancellationToken ct)
    {throw new NotImplementedException(); }

    private static async Task<IResult> GetFeed(
        [FromServices] IPostRepository posts,
        [FromServices] IUserRepository currentUser,
        CancellationToken ct)
    { throw new NotImplementedException();}
    
    private static async Task<IResult> DeletePost(
        Guid id,
        [FromServices] IPostRepository posts,
        [FromServices] IUserRepository currentUser,
        CancellationToken ct)
    {throw new NotImplementedException();}
}