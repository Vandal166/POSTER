namespace Application.DTOs;

public record PostDto(Guid Id, string AuthorUsername, string AuthorAvatarPath, string Content, DateTime CreatedAt, 
    int LikesCount = 0, int CommentsCount = 0, int ViewsCount = 0);