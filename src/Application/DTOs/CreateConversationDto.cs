namespace Application.DTOs;

public record CreateConversationDto(string Name, Guid ProfilePictureFileID);

public record UpdateConversationDto(Guid Id, string Name, Guid? ProfilePictureID);