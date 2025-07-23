using Microsoft.AspNetCore.Authentication;

namespace Infrastructure.Auth;

public static class CookiePropertiesFactory
{
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