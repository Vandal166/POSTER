namespace Application.DTOs;

public record CreatePostDto(string Content, Guid? VideoFileID = null);