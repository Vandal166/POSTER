using Domain.ValueObjects;

namespace Domain.Entities;

public class User
{
    public Guid ID { get; set; }
    public UserName Username { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public string? PasswordHash { get; set; } = null;
    
    //public List<Post> Posts { get; set; } = new List<Post>();
}