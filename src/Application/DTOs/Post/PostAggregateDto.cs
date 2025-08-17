namespace Application.DTOs;

public sealed class PostAggregateDto
{
    public PostDto Post { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsLiked { get; set; }
}