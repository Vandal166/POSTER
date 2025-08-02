namespace Application.DTOs;

public record PostDto(
    Guid Id,
    string AuthorUsername,
    string AuthorAvatarPath,
    string Content,
    DateTime CreatedAt, Guid? VideoFileID = null)
{
    public bool IsTruncated => Content.Length > 300;
}