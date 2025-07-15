using FluentResults;

namespace Domain.Entities;
public record CreatePostDto(string Content);
public record PostDto(Guid Id, string AuthorUsername, string Content, DateTime CreatedAt);
/*
 *<Avatar><Username of the author> <Date of post>
 * <Content>
 * <Likes> <Comments> <Views>-count
 * <Comments upon expanding>
 */
public sealed class Post : AuditableEntity
{
    public Guid AuthorID { get; private set; }
    public User Author { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    
    // EF will populate this
    public IReadOnlyCollection<Comment> Comments => _comments; // comments on the post
    private readonly List<Comment> _comments = new();
    
    private Post(){}
    
    public static Result<Post> Create(User? author, string content)
    {
        if (author is null) 
            return Result.Fail<Post>("Author is required.");
        
        if (string.IsNullOrWhiteSpace(content) || content.Length > 280)
            return Result.Fail<Post>("Content must be between 1 and 280 characters.");
        
        var post = new Post
        {
            ID        = Guid.NewGuid(),
            AuthorID  = author.ID,
            Author    = author,
            Content   = content,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(post);
    }
}