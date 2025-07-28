using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostLikeService
{
    // Returns true if the post was liked, false if it was unliked
    Task<PostLikesDto> ToggleLikeAsync(Guid postID, Guid userID, CancellationToken ct = default);
    Task<Result> LikePostAsync(Guid postID, Guid userID, CancellationToken ct = default);
    Task<Result> UnlikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
}