using FluentResults;

namespace Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = null!;
    private User() { }
    
    public static Result<User> Create(Guid keycloakID, string username)
    {
        if (username.Length < 3 || username.Length > 50)
        {
            return Result.Fail<User>("Username must be between 3 and 50 characters long.");
        }
        
        var user = new User
        {
            ID           = keycloakID,
            Username     = username,
        };
        
        return Result.Ok(user);
    }
    
    // change password etc
}