namespace Application.DTOs;

public sealed class CreateMessageViewModel
{
    public required Guid ConversationId { get; init; } // The conversation ID to which the message is being sent
    public string Content { get; init; } // The content of the message
}