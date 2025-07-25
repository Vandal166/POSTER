using System.IdentityModel.Tokens.Jwt;
using Application.Contracts;
using System.Security.Claims;
using Domain.ValueObjects;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Web.Services;

// Used to get the current user ID from the HTTP context(currently logged in user).
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    private ClaimsPrincipal User => _http.HttpContext?.User ?? new ClaimsPrincipal();
    
    //--- USER PROPERTIES ---//
    public string ID => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
    public bool Enabled =>
        bool.TryParse(User.FindFirst("enabled")?.Value, out var enabled) && enabled;

    public string Username =>
        User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    
    public string Email =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public Dictionary<string, string[]> Attributes =>
        User.Claims
            .Where(c => c.Type.StartsWith("attr:")) // Convention: store as attr:key
            .GroupBy(c => c.Type.Substring(5))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToArray());

    public List<string> RealmRoles =>
        User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    
    
   public bool HasClaim(string type, string value)
   {
       return User.HasClaim(c => 
           string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(c.Value, value, StringComparison.OrdinalIgnoreCase));
   }
    
    public async Task RefreshClaims(ClaimsPrincipal newPrincipal)
    {
        if (_http.HttpContext == null)
            throw new InvalidOperationException("HTTP context is not available.");
        
        var existingAuthResult = await _http.HttpContext.AuthenticateAsync();
        if (!existingAuthResult.Succeeded)
            throw new InvalidOperationException("No authenticated user to refresh claims for.");
        
        // re‑issuing the cookie
        var newProps = existingAuthResult.Properties ?? new AuthenticationProperties
        {
            IsPersistent = true
        };
        newProps.IssuedUtc = DateTimeOffset.UtcNow; // updating the issued time
        
        await _http.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, newProps);
    }
}
