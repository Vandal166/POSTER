namespace Application.Contracts;

public interface IFollowNotifier
{
    Task NotifyFollowedAsync(Guid followerID, Guid followingID, CancellationToken ct = default);

    Task NotifyPostCreatedAsync(Guid postID, IEnumerable<Guid> followerIDs, CancellationToken ct = default);

    Task NotifyCommentOnPostReceivedAsync(Guid authorOfPostID, CancellationToken ct = default);
    
    Task NotifyCommentOnCommentReceivedAsync(Guid authorOfParentCommentId, CancellationToken ct = default);
}