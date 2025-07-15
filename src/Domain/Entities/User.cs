using Domain.ValueObjects;
using FluentResults;

namespace Domain.Entities;

public class User : AuditableEntity
{
    public UserName Username { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public string? PasswordHash { get; set; } = null;
    
    private User() { }
    
    public static Result<User> Create(UserName username, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Fail<User>("Password is required.");
                
        if(passwordHash.Length < 6 || passwordHash.Length > 256)
            return Result.Fail<User>("Password must be between 6 and 256 characters.");
            
        var user = new User
        {
            ID           = Guid.NewGuid(),
            Username     = username,
            Email        = email,
            PasswordHash = passwordHash
        };
        
        return Result.Ok(user);
    }
    
    // change password etc
}