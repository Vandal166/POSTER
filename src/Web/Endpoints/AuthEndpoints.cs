using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");
        group.MapPost("/register", RegisterUser);
        group.MapPost("/login", LoginUser);
        
        return app;
    }
    
    public static async Task<IResult> RegisterUser([FromBody] RegisterUserDto dto, [FromServices] PosterDbContext db, [FromServices] IValidator<RegisterUserDto> validator)
    {
        // 1. Validate
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        // 2. Check uniqueness
        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
            return Results.Conflict(new { message = "Username already taken" });

        // 3. Create & save
        var user = new Domain.Entities.User
        {
            ID           = Guid.NewGuid(),
            Username     = dto.Username,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Created($"/{user.ID}", new { user.ID });
    }
    
    public static async Task<IResult> LoginUser([FromBody] LoginUserDto dto, [FromServices] PosterDbContext db, [FromServices] IValidator<LoginUserDto> validator, 
        [FromServices] IConfiguration jwtConfig)
    {
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Results.Unauthorized();

        // 3. Build JWT
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
        };
        
        var jwtSection = jwtConfig.GetSection("JwtSettings");
        var secret = Encoding.UTF8.GetBytes(jwtSection["Secret"]);
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresInMinutes = jwtSection["ExpiresInMinutes"];
        
        var creds  = new SigningCredentials(new SymmetricSecurityKey(secret),
            SecurityAlgorithms.HmacSha256);
        var token  = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(int.Parse(expiresInMinutes)),
            signingCredentials: creds
        );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = jwtToken });
    }
}