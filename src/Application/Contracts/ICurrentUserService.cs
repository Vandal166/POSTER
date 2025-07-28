using System.Security.Claims;

namespace Application.Contracts;

public interface ICurrentUserService
{
    Guid ID { get; }
    bool Enabled { get; }
    string Username { get; }
    string Email { get; }
    string AvatarPath { get; }
    
    bool HasClaim(string type, string value);
    Task RefreshClaims(ClaimsPrincipal newPrincipal);
}
