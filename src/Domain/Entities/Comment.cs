using FluentResults;

namespace Domain.Entities;
/*
 *<Avatar><Username of the author> <Date of comment>
 * <Content>
 * <Likes> <Comments> <Views>-count
 * <Even more Comments upon expanding>
 */
public sealed class Comment : AuditableEntity
{
    public Guid PostID { get; private set; }
    public Post Post { get; private set; } = null!; // the post that was commented on
    public Guid AuthorID { get; private set; }
    public User Author { get; private set; } = null!; // the user who made the comment
    public Guid? ParentCommentID { get; private set; }  // null for top most‐level, so if this is a reply to another comment, this will be the ID of the parent comment
    public Comment? ParentComment   { get; private set; }
   
    public string Content { get; set; } = null!; // content of the comment
    
    public IReadOnlyCollection<Comment> Replies => _replies;
    private readonly List<Comment> _replies = new();
    
    private Comment() { }
    
    public static Result<Comment> Create(Guid postID, Guid authorID, string content, Comment? parent = null)
    {
        if (postID == Guid.Empty)
            return Result.Fail<Comment>("Post is required.");
        if (authorID == Guid.Empty)
            return Result.Fail<Comment>("Author is required.");
      
        var comment = new Comment
        {
            ID              = Guid.NewGuid(),
            PostID          = postID,
            AuthorID        = authorID,
            Content         = content,
            CreatedAt       = DateTime.UtcNow,
            ParentComment   = parent,
            ParentCommentID = parent?.ID
        };
        return Result.Ok(comment);
    }
}