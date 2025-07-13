using System.Security.Claims;
using System.Text;
using Application.Contracts;
using Application.DTOs;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
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
    
    public static async Task<IResult> RegisterUser([FromBody] RegisterUserDto dto, [FromServices] IAuthService auth)
    {
        var result = await auth.RegisterAsync(dto);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));

        return Results.Created($"/users/{result.Value}", new { result.Value });
        /*// dto validation
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());
       
        var userNameResult = UserName.Create(dto.Username);
        // var emailResult = Email.Create(dto.Email);
        
        var errors = new Dictionary<string, string[]>();
        if (userNameResult.IsFailed)
            errors["Username"] = userNameResult.Errors.Select(e => e.Message).ToArray();
        // if (emailResult.IsFailed)
        //     errors["Email"] = emailResult.Errors.Select(e => e.Message).ToArray();
        
        if (errors.Any())
            return Results.ValidationProblem(errors);
        
        // using the value object value now
        var userName = userNameResult.Value;
        // var email = emailResult.Value;
        
        // uniqueness check
        if (await db.Users.AnyAsync(u => u.Username == userName))
            return Results.Conflict(new { message = "Username already taken" });
        
        // 3. Create & save
        var user = new Domain.Entities.User
        {
            ID           = Guid.NewGuid(),
            Username     = userName,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
//TODO transaction?
        return Results.Created($"/{user.ID}", new { user.ID });*/
    }
    
    public static async Task<IResult> LoginUser([FromBody] LoginUserDto dto, [FromServices] IAuthService auth)
    {
        var result = await auth.LoginAsync(dto);
        if (result.IsFailed)
            return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Message, e => new[] { e.Message }));
        
        return Results.Ok(new { token = result.Value });
        /*var validation = await validator.ValidateAsync(dto);
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

        //TODO abstract token generation behind an interface in the Application layer (e.g., ITokenGenerator) and inject it into Web
        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(expiresInMinutes)),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };
        
        var jwtToken = tokenHandler.CreateToken(tokenDescriptor);

        return Results.Ok(new { token = jwtToken });*/
    }
}