namespace Application.DTOs;

public record CreateConversationDto(string Name, Guid? ProfilePictureFileID = null);