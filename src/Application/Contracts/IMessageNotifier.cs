namespace Application.Contracts;

public interface IMessageNotifier
{
    Task NotifyMessageCreatedAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task NotifyMessageDeletedAsync(Guid conversationID, Guid messageId, CancellationToken ct = default);
    
    Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default);
}

public interface IConversationNotifier
{
    Task NotifyConversationCreatedAsync(Guid conversationID, IEnumerable<Guid> participantIDs, CancellationToken ct = default);
    
    Task NotifyConversationUpdatedAsync(Guid conversationID, CancellationToken ct = default);
    
    Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default);

    Task NotifyMessageCreated(Guid conversationId, CancellationToken ct = default);
}