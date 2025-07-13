using System.Security.Claims;
using System.Text;
using Application.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Infrastructure.Auth;

public class JwtTokenGeneratorService : ITokenGenerator
{
    public string GenerateToken(User user, IConfiguration configuration)
    {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username.Value)
        };
        
        var jwtSection = configuration.GetSection("JwtSettings");
        var secret = Encoding.UTF8.GetBytes(jwtSection["Secret"]);
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresInMinutes = jwtSection["ExpiresInMinutes"];
        
        var signingCredentials  = new SigningCredentials(new SymmetricSecurityKey(secret),
            SecurityAlgorithms.HmacSha256);

        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(expiresInMinutes)),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials
        };
        
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}