namespace Application.DTOs;

//TODO why class and not struct?
public record CommentDto(Guid Id, string AuthorUsername, string Content, DateTime CreatedAt);