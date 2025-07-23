namespace Application.DTOs;

public record CommentDto(Guid Id, string AuthorUsername, string Content, DateTime CreatedAt);