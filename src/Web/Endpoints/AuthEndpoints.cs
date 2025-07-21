using System.Net.Http.Headers;
using System.Security.Claims;
using Application.Contracts;
using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Web.Endpoints;

public class AuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
        
        string? accessToken = await httpContext.GetTokenAsync("Keycloak", "access_token");
        
        if (string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");
        
        group.MapPost("/register", RegisterUser)
            .WithName("RegisterUser")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .AllowAnonymous();
        
        group.MapGet("/login", () => Results.Redirect("/Login"))
            .AllowAnonymous();
        
        group.MapPost("/login", LoginUser)
            .WithName("LoginUser")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status302Found)
            .ProducesValidationProblem()
            .AllowAnonymous();
        
        group.MapGet("/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        })
        .RequireAuthorization();
        
        
        app.MapPost("/api/v1/auth/complete-profile", async (
                CompleteProfileDto dto,
                HttpContext ctx,
                IAuthService auth) =>
            {
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await auth.CompleteProfileAsync(userId, dto.Username);
                if (result.IsFailed)
                    return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
               
                return Results.Redirect("/");
            })
            .RequireAuthorization();

    }
    

    private static async Task<IResult> RegisterUser([FromBody] RegisterUserDto dto, [FromServices] IAuthService auth, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(dto, cancellationToken);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));

        return Results.Created();
    }

    private static async Task<IResult> LoginUser([FromBody] LoginUserDto dto, HttpContext ctx, [FromServices] IAuthService auth)
    {
        /*return TypedResults.Challenge(
            new AuthenticationProperties{RedirectUri = "/"}, ["Keycloak"]);*/
        var result = await auth.LoginAsync(dto, ctx);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        var (principal, props) = result.Value;

        // 1) Check profileCompleted claim
        var profileCompleted = principal.Claims
            .FirstOrDefault(c => c.Type == "profileCompleted")?.Value;

        if (!string.Equals(profileCompleted, "true", StringComparison.OrdinalIgnoreCase))
        {
            // Don't sign in yet—just store the tokens in a temp cookie or session
            // Then redirect them to your complete-profile UI
            // (You can still sign in if you like, so the UI can fetch the tokens)
            await ctx.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props
            );
            return Results.Redirect("/complete-profile");
        }

        // 2) Normal full sign‑in and 200 OK
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        
        return Results.Ok();
    }
}