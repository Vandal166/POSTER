namespace Application.DTOs;

public record CreateMessageDto(Guid ConversationID, string Content, Guid? VideoFileID = null, Guid[]? ImageFileIDs = null);