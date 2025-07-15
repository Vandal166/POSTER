using System.IdentityModel.Tokens.Jwt;
using Application.Contracts;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security;

// Used to get the current user ID from the HTTP context(currently logged in user).
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
        => _http = http;

    public Guid UserID
    {
        get
        {
            var user = _http.HttpContext?.User
                       ?? throw new InvalidOperationException("No http context");

            // subject contain user ID and UserName
            var subject = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(subject, out var id))
                return id;

            throw new InvalidOperationException("User ID claim is not a GUID.");
        }
    }
}
