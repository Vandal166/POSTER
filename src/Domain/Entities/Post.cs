namespace Domain.Entities;

public class Post
{
    public Guid ID { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Guid UserID { get; set; }
    public User User { get; set; } = null!;
}