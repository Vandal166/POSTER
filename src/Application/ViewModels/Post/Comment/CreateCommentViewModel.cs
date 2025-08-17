namespace Application.ViewModels;

public sealed class CreateCommentViewModel
{
    public required Guid EntityId { get; init; } // The entity ID (Post or Comment) to which the comment is being made
    public Guid? ParentCommentId { get; init; } = null; // Optional parent comment ID for nested comments
    public string Content { get; init; }
}