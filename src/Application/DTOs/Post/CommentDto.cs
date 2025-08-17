namespace Application.DTOs;

public record CommentDto(Guid Id, string AuthorUsername, string AuthorAvatarPath, string Content, DateTime CreatedAt);