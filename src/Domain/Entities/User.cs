using System;

namespace Domain.Entities;

public class User
{
    public Guid ID { get; set; }
    public string? Username { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? PasswordHash { get; set; } = null;
    
    public List<Post> Posts { get; set; } = new List<Post>();
}