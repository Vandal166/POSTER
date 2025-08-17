using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

[Authorize]
internal sealed class ConversationNotificationHub : Hub
{
    public async Task JoinConversationGroups(IEnumerable<Guid> conversationIds)
    {
        foreach (var id in conversationIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
        }
    }
}