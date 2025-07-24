using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostLikeService
{
    Task<Result> LikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    Task<Result> UnlikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
}