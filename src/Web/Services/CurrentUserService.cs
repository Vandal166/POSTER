using System.IdentityModel.Tokens.Jwt;
using Application.Contracts;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Web.Services;

// Used to get the current user ID from the HTTP context(currently logged in user).
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    private ClaimsPrincipal User => _http.HttpContext?.User ?? new ClaimsPrincipal();
    
    //--- USER PROPERTIES ---//
    public Guid ID => GetUserId();
    
    public bool IsAuthenticated =>
        User.Identity?.IsAuthenticated ?? false;
    
    public string Username =>
        User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public string AvatarPath =>
        User.FindFirst("avatarPath")?.Value ?? "uploads/avatars/default.png";
    
    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                      User.FindFirst(JwtRegisteredClaimNames.Sub);
        
        return idClaim != null && Guid.TryParse(idClaim.Value, out var userId) ? userId : Guid.Empty;
    }
    
    
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
