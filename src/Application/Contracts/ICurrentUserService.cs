using System.Security.Claims;
using Domain.ValueObjects;

namespace Application.Contracts;

public interface ICurrentUserService
{
    string ID { get; }
    bool Enabled { get; }
    string Username { get; }
    string Email { get; }
    //email etc
    Dictionary<string, string[]> Attributes { get; }
    List<string> RealmRoles { get;}
    bool HasClaim(string type, string value);
    Task RefreshClaims(ClaimsPrincipal newPrincipal);
    /*Guid UserID { get; }
    UserName UserName { get; }*/
}
