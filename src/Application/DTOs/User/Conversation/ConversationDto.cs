namespace Application.DTOs;

// conversation id, name, the profile picture of the user that created the conversation, and the last message timestamp
public record ConversationDto(Guid Id, string Name, Guid ProfilePictureID, string? LastMessageContent, DateTime LastMessageAt, DateTime CreatedAt, Guid CreatedByID)
{
    public bool ShouldTruncate(string? content, int maxLength = 40) => content?.Length > maxLength;
}