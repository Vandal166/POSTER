using FluentResults;

namespace Domain.Entities;

public class PostView : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Post Post { get; private set; } = null!; // the post that was viewed
    public Guid UserID { get; private set; }
    public User User { get; private set; } = null!; // the unique-user who viewed the post
    
    private PostView() { }
    public static Result<PostView> Create(Post? post, User? user)
    {
        if (post is null) 
            return Result.Fail<PostView>("Post is required.");
        if (user is null)
            return Result.Fail<PostView>("User is required.");
        
        var view = new PostView
        {
            ID      = Guid.NewGuid(),
            PostID  = post.ID,
            Post    = post,
            UserID  = user.ID,
            User    = user,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(view);
    }
}