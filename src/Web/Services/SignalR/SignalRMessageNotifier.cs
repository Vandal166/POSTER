using Application.Contracts;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

// This is the SignalR message notifier that sends notifications to clients who are connected to the /messageHub - in Detail.cshtml
internal sealed class SignalRMessageNotifier : IMessageNotifier
{
    private readonly IHubContext<MessageNotificationHub> _hubContext;

    public SignalRMessageNotifier(IHubContext<MessageNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageCreatedAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationId.ToString())
            .SendAsync("MessageCreated", conversationId, messageId, ct);
    }

    public async Task NotifyMessageDeletedAsync(Guid conversationID, Guid messageId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationID.ToString())
            .SendAsync("MessageDeleted", conversationID, messageId, ct);
    }

    public async Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationID.ToString())
            .SendAsync("ConversationDeleted", conversationID, ct);
    }

    public async Task NotifyParticipantRemovedAsync(Guid conversationID, Guid participantID, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationID.ToString())
            .SendAsync("ParticipantRemoved", conversationID, participantID, ct);
    }
}