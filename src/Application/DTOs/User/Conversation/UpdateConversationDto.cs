namespace Application.DTOs;

public record UpdateConversationDto(Guid Id, string Name, Guid? ProfilePictureID);