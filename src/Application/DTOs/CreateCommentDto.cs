namespace Application.DTOs;

public record CreateCommentDto(string Content);

public class CreateCommentViewModel
{
    public required Guid PostId { get; init; }
    public string Content { get; init; }
}