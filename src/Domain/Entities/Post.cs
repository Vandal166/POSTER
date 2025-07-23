using FluentResults;

namespace Domain.Entities;

/*
 *<Avatar><Username of the author> <Date of post>
 * <Content>
 * <Likes> <Comments> <Views>-count
 * <Comments upon expanding>
 */
public sealed class Post : AuditableEntity
{
    public Guid AuthorID { get; private set; } // Keycloak ID of the author
    public User Author { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    
    // EF will populate this
    public IReadOnlyCollection<Comment> Comments => _comments; // comments on the post
    private readonly List<Comment> _comments = new();
    
    private Post(){}
    
    public static Result<Post> Create(Guid authorID, string content)
    {
        if (authorID == Guid.Empty)
            return Result.Fail<Post>("Author ID cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(content) || content.Length > 280)
            return Result.Fail<Post>("Content must be between 1 and 280 characters.");
        
        var post = new Post
        {
            ID        = Guid.NewGuid(),
            AuthorID  = authorID,
            Content   = content,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(post);
    }
}