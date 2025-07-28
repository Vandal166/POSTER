namespace Application.DTOs;

public record PostDto(
    Guid Id,
    string AuthorUsername,
    string AuthorAvatarPath,
    string Content,
    DateTime CreatedAt)
{
    public bool IsTruncated => Content.Length > 300;
}
    
public class PostAggregateDto
{
    public PostDto Post { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsLiked { get; set; }
}