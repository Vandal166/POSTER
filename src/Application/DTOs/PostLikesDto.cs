namespace Application.DTOs;

public record PostLikesDto(Guid PostID, int LikesCount, bool IsLiked);

public record CommentLikesDto(Guid CommentID, int LikesCount, bool IsLiked);