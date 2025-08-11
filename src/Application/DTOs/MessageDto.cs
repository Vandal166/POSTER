namespace Application.DTOs;

public record MessageDto(Guid Id, Guid ConversationID, Guid AuthorID, string AuthorUsername, string AuthorAvatarPath, string Content, DateTime CreatedAt);