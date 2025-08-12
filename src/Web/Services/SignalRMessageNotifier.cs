using Application.Contracts;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

public sealed class SignalRConversationNotifier : IConversationNotifier
{
    private readonly IHubContext<ConversationNotificationHub> _hubContext;
    public SignalRConversationNotifier(IHubContext<ConversationNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyConversationCreatedAsync(Guid conversationID, IEnumerable<Guid> participantIDs, CancellationToken ct = default)
    {
        foreach (var participantID in participantIDs)
        {
            await _hubContext.Clients.User(participantID.ToString())
                .SendAsync("ConversationCreated", conversationID, ct);
        }
    }

    public async Task NotifyConversationUpdatedAsync(Guid conversationID, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task NotifyConversationDeletedAsync(Guid conversationID, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationID.ToString())
            .SendAsync("ConversationDeleted", conversationID, ct);
    }

    public async Task NotifyMessageCreated(Guid conversationId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(conversationId.ToString())
            .SendAsync("MessageCreated", conversationId, ct);
    }
}

public class SignalRMessageNotifier : IMessageNotifier
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
}