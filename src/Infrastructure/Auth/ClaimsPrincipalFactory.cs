using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Infrastructure.Auth;

public static class ClaimsPrincipalFactory
{
    public static ClaimsPrincipal BuildClaims(KeycloakUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("profileCompleted", user.Attributes?["profileCompleted"]?.FirstOrDefault() ?? "false")
        };

        foreach (var role in user.RealmRoles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

}