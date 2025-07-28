namespace Application.DTOs;

public record PostLikesDto(Guid PostID, int LikesCount, bool IsLiked);

public record PostCommentsDto(Guid PostID, int CommentsCount)
{
    public bool IsEmpty => CommentsCount == 0;
}

public record PostViewsDto(Guid PostID, int ViewsCount)
{
    public bool IsEmpty => ViewsCount == 0;
}