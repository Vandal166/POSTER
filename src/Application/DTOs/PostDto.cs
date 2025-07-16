namespace Application.DTOs;

public record PostDto(Guid Id, string AuthorUsername, string Content, DateTime CreatedAt);