namespace Application.DTOs;

public record PostLikesDto(Guid PostID, int LikesCount, bool IsLiked);