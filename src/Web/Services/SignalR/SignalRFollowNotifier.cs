using Application.Contracts;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

// This is the SignalR follow notifier that sends notifications to followers when a user they follow performs actions like following another user or creating a post.
internal sealed class SignalRFollowNotifier : IFollowNotifier
{
    private readonly IHubContext<FollowNotificationHub> _hubContext;

    public SignalRFollowNotifier(IHubContext<FollowNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyFollowedAsync(Guid followerID, Guid followingID, CancellationToken ct = default)
    {
        await _hubContext.Clients.User(followingID.ToString())
            .SendAsync("Followed", followerID, followingID, ct);
    }

    public async Task NotifyPostCreatedAsync(Guid postID, IEnumerable<Guid> followerIDs, CancellationToken ct = default)
    {
        foreach (var followerID in followerIDs)
        {
            await _hubContext.Clients.User(followerID.ToString())
                .SendAsync("PostCreated", postID, followerID, ct);
        }
    }
    
    // sending an notification to the author of the post that an comment has been made on their post
    public async Task NotifyCommentOnPostReceivedAsync(Guid authorOfPostID, CancellationToken ct = default)
    {
        await _hubContext.Clients.User(authorOfPostID.ToString())
            .SendAsync("CommentOnPostReceived", authorOfPostID.ToString(), ct);
    }

    public async Task NotifyCommentOnCommentReceivedAsync(Guid authorOfParentCommentId, CancellationToken ct = default)
    {
        await _hubContext.Clients.User(authorOfParentCommentId.ToString())
            .SendAsync("CommentOnCommentReceived", authorOfParentCommentId.ToString(), ct);
    }
}