using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

[Authorize]
public sealed class FollowNotificationHub : Hub
{
    public async Task JoinFollowGroups(IEnumerable<Guid> userIds)
    {
        foreach (var id in userIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
        }
    }
}