using FluentResults;

namespace Domain.Entities;

public sealed class PostView : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Post Post { get; private set; } = null!; // the post that was viewed
    public Guid UserID { get; private set; }
    public User User { get; private set; } = null!; // the unique-user who viewed the post
    
    private PostView() { }
    public static Result<PostView> Create(Guid postID, Guid userID)
    {
        if (postID == Guid.Empty)
            return Result.Fail("Post ID cannot be empty.");
        
        if (userID == Guid.Empty)
            return Result.Fail("User ID cannot be empty.");
        
        var view = new PostView
        {
            ID      = Guid.NewGuid(),
            PostID  = postID,
            UserID  = userID,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(view);
    }
}