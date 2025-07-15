using FluentResults;

namespace Domain.Entities;

public class PostLike : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Post Post { get; private set; } = null!; // the post that was liked
    public Guid UserID { get; private set; }
    public User User { get; private set; } = null!; // the user who liked the post
    
    private PostLike() { }
    
    public static Result<PostLike> Create(Post? post, User? user)
    {
        if (post is null) 
            return Result.Fail<PostLike>("Post is required.");
        if (user is null)
            return Result.Fail<PostLike>("User is required.");
        
        var like = new PostLike
        {
            ID      = Guid.NewGuid(),
            PostID  = post.ID,
            Post    = post,
            UserID  = user.ID,
            User    = user,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(like);
    }
}