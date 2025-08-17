namespace Application.DTOs;

public record CommentLikesDto(Guid CommentID, int LikesCount, bool IsLiked);