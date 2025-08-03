using FluentResults;

namespace Domain.Entities;

public sealed class PostImage : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Guid ImageFileID { get; private set; }
    public Post Post { get; private set; } = null!;
    
    private PostImage() { }
    
    public static Result<PostImage> Create(Guid postID, Guid imageFileID)
    {
        if (postID == Guid.Empty)
            return Result.Fail<PostImage>("Post ID cannot be empty.");
        
        if (imageFileID == Guid.Empty)
            return Result.Fail<PostImage>("Image File ID cannot be empty.");
        
        var postImage = new PostImage
        {
            ID = Guid.NewGuid(),
            PostID = postID,
            ImageFileID = imageFileID,
            CreatedAt = DateTime.UtcNow
        };
        
        return Result.Ok(postImage);
    }
}