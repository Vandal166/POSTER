namespace Application.DTOs;

public sealed class CommentAggregateDto
{
    public CommentDto Comment { get; set; }
    public required Guid PostId { get; init; } // The ID of the post to which this comment belongs
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
}