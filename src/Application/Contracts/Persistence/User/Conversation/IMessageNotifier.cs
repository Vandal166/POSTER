namespace Application.Contracts;

public interface IMessageNotifier
{
    Task NotifyMessageCreatedAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task NotifyMessageDeletedAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default);
    
    Task NotifyParticipantRemovedAsync(Guid conversationID, Guid participantID, CancellationToken ct = default);
}