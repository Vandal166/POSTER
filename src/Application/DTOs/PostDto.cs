namespace Application.DTOs;

public record PostDto(
    Guid Id,
    string AuthorUsername,
    string AuthorAvatarPath,
    string Content,
    DateTime CreatedAt, Guid? VideoFileID = null, Guid[]? ImageFileIDs = null)
{
    public bool IsTruncated => Content.Length > 300;
}

// conversation id, name, the profile picture of the user that created the conversation, and the last message timestamp
public record ConversationDto(Guid Id, string Name, string ProfilePicturePath, string Content, DateTime LastMessageAt)
{
    public bool ShouldTruncate(string content, int maxLength = 40) => content.Length > maxLength;
}