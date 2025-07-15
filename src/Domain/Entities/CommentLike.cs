using FluentResults;

namespace Domain.Entities;

public class CommentLike : AuditableEntity
{
    public Guid CommentID { get; private set; }
    public Comment Comment { get; private set; } = null!; // the comment that was liked
    public Guid UserID { get; private set; }
    public User User { get; private set; } = null!; // the user who liked the comment
    
    private CommentLike() { }
    
    public static Result<CommentLike> Create(Comment? comment, User? user)
    {
        if (comment is null) 
            return Result.Fail<CommentLike>("Comment is required.");
        if (user is null)
            return Result.Fail<CommentLike>("User is required.");
        
        var like = new CommentLike
        {
            ID      = Guid.NewGuid(),
            CommentID = comment.ID,
            Comment = comment,
            UserID  = user.ID,
            User    = user,
            CreatedAt = DateTime.UtcNow
        };
        return Result.Ok(like);
    }
}