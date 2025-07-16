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
            .Produces<List<CommentDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{commentID:guid}", DeleteComment)
            .WithName("DeleteComment")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        
        return app;
    }
    
}