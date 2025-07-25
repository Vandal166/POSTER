using FluentResults;

namespace Domain.Entities;

public class User : AuditableEntity
{
    public string AvatarPath { get; set; } = string.Empty; // '/wwwroot/uploads/avatars/12345.png'
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
            AvatarPath = "uploads/avatars/default.png", // default avatar
            Username     = username,
        };
        
        return Result.Ok(user);
    }
    
    // change password etc
}