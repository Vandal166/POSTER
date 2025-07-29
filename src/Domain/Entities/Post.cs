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
    
    public IReadOnlyCollection<PostLike> Likes => _likes; // likes on the post
    private readonly List<PostLike> _likes = new();
    
    public IReadOnlyCollection<PostView> Views => _views; // views on the post
    private readonly List<PostView> _views = new();
    
    private Post(){}
    
    public static Result<Post> Create(Guid authorID, string content)
    {
        if (authorID == Guid.Empty)
            return Result.Fail<Post>("Author ID cannot be empty.");
        
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