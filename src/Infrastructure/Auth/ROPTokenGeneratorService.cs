using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Application.Contracts;
using Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Infrastructure.Auth;

public class ROPTokenGeneratorService : ITokenGenerator<HttpResponseMessage>
{
    public static FormUrlEncodedContent CreateFormContent(Dictionary<string, string> parameters)
    {
        return new FormUrlEncodedContent(parameters);
    }
    
    // Generates a token using the Resource Owner Password Credentials (ROP) grant type.
    public async Task<HttpResponseMessage> GenerateTokenAsync(HttpClient client, FormUrlEncodedContent formUrlEncodedContent, CancellationToken cancellationToken = default)
    {
        //TODO use Authorization Code with PKCE instead of Resource Owner Password grant
        // 1) Exchange user creds for code (Authorization Code with PKCE is recommended).
        //    For simplicity here we use Resource Owner Password grant (not recommended for prod):
        return await client.PostAsync(
            "protocol/openid-connect/token",
            formUrlEncodedContent,
            cancellationToken
        );
    }
   
    public async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // sending the request to the Keycloak server
        return await client.SendAsync(request, cancellationToken);
    }

    public static async Task<(string? idToken, string? accessToken, string? refreshToken)> ParseTokenResponeAsync(HttpResponseMessage res)    
    {
        var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        // Some grants (ROP) do not return an ID token.
        string? idToken = json.TryGetProperty("id_token", out var idProp) ? idProp.GetString() : null;
        var accessToken = json.GetProperty("access_token").GetString() 
                          ?? throw new InvalidOperationException("Missing access_token");
        var refreshToken = json.GetProperty("refresh_token").GetString() 
                           ?? throw new InvalidOperationException("Missing refresh_token");
        return (idToken, accessToken, refreshToken);
    }
}

public static class KeycloakClientService
{
    public static object CreateClient(RegisterUserDto dto)
    {
        var user = new {
            username = dto.Email,
            email    = dto.Email,
            enabled  = true,
            attributes = new {
                profileCompleted = new[] { "false" }
            },
            credentials = new[] {
                new { type = "password", value = dto.Password, temporary = false }
            }
        };
        return user;
    }
    
    public static ClaimsPrincipal BuildClaims(string? jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken))
            throw new ArgumentException("JWT token cannot be null or empty.");
        
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(jwtToken);
        var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var userPrincipal = new ClaimsPrincipal(identity);
        return userPrincipal;
    }
    
    public static AuthenticationProperties BuildCookie(string accessToken, string refreshToken, string? idToken)
    {
        if(string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Tokens cannot be null or empty.");
        
        return new AuthenticationProperties(new Dictionary<string, string> {
                [".Token.access_token"]  = accessToken,
                [".Token.refresh_token"] = refreshToken,
                [".Token.id_token"]      = idToken
            })
            { IsPersistent = true };
    }
}