using System.Security.Claims;

namespace Application.Contracts;

public interface ICurrentUserService
{
    Guid ID { get; }
    bool IsAuthenticated { get; }
    string Username { get; }
    string AvatarPath { get; }
    
    bool HasClaim(string type, string value);
    Task RefreshClaims(ClaimsPrincipal newPrincipal);
}
