namespace Application.Contracts;

public interface IConversationNotifier
{
    Task NotifyConversationCreatedAsync(Guid conversationID, IEnumerable<Guid> participantIDs, CancellationToken ct = default);

    Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default);

    Task NotifyMessageCreated(Guid conversationId, CancellationToken ct = default);

    Task NotifyParticipantRemovedAsync(Guid conversationID, Guid participantID, CancellationToken ct = default);
}