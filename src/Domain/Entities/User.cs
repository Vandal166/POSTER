using System.Text.RegularExpressions;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = null!;
    //public string Email { get; set; } = null!;
    //public string? PasswordHash { get; set; } = null;
    
    private User() { }
    
    public static Result<User> Create(Guid keycloakID, string username)//, string email)//, string passwordHash)
    {
        /*if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Fail<User>("Password is required.");
                
        if(passwordHash.Length < 6 || passwordHash.Length > 256)
            return Result.Fail<User>("Password must be between 6 and 256 characters.");
            */
            
        if (username.Length < 3 || username.Length > 50)
        {
            return Result.Fail<User>("Username must be between 3 and 50 characters long.");
        }
        /*if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return Result.Fail<User>("Invalid email format.");
        }*/
        var user = new User
        {
            ID           = keycloakID,
            Username     = username,
            //Email        = email,
            //PasswordHash = passwordHash
        };
        
        return Result.Ok(user);
    }
    
    // change password etc
}