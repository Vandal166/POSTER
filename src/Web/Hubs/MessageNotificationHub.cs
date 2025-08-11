using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

public sealed class MessageNotificationHub : Hub
{
    public async Task JoinGroup(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }
}