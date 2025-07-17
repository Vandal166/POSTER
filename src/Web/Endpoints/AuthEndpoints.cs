using Application.Contracts;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");
        
        group.MapPost("/register", RegisterUser)
            .WithName("RegisterUser")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        
        group.MapPost("/login", LoginUser)
            .WithName("LoginUser")
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        
        return app;
    }

    private static async Task<IResult> RegisterUser([FromBody] RegisterUserDto dto, [FromServices] IAuthService auth, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(dto, cancellationToken);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));

        return Results.Created($"/users/{result.Value}", new { result.Value });
    }

    private static async Task<IResult> LoginUser([FromBody] LoginUserDto dto, [FromServices] IAuthService auth)
    {
        var result = await auth.LoginAsync(dto);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        return Results.Ok(new { token = result.Value });
    }
}