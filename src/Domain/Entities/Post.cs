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
    public Guid? VideoFileID { get; private set; } // Optional video file ID for the post
    
    // EF will populate this
    public IReadOnlyCollection<PostImage> Images => _images;
    private readonly List<PostImage> _images = new();
    
    public IReadOnlyCollection<Comment> Comments => _comments; // comments on the post
    private readonly List<Comment> _comments = new();
    
    public IReadOnlyCollection<PostLike> Likes => _likes; // likes on the post
    private readonly List<PostLike> _likes = new();
    
    public IReadOnlyCollection<PostView> Views => _views; // views on the post
    private readonly List<PostView> _views = new();
    
    private Post(){}
    
    public static Result<Post> Create(Guid authorID, string content, Guid? videoFileID = null)
    {
        if (authorID == Guid.Empty)
            return Result.Fail<Post>("Author ID cannot be empty.");
        
        var post = new Post
        {
            ID        = Guid.NewGuid(),
            AuthorID  = authorID,
            Content   = content,
            CreatedAt = DateTime.UtcNow,
            VideoFileID = videoFileID
        };
        return Result.Ok(post);
    }
}