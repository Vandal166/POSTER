using FluentResults;

namespace Domain.Entities;

public class PostLike : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Post Post { get; private set; } = null!; // the post that was liked
    public Guid UserID { get; private set; }
    public User User { get; private set; } = null!; // the user who liked the post
    
    private PostLike() { }
    
    public static Result<PostLike> Create(Guid postID, Guid userID)
    {
        if (postID == Guid.Empty)
            return Result.Fail<PostLike>("Post is required.");
        if (userID == Guid.Empty)
            return Result.Fail<PostLike>("User is required.");
        
        var like = new PostLike
        {
            ID      = Guid.NewGuid(),
            PostID  = postID,
            UserID  = userID,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(like);
    }
}