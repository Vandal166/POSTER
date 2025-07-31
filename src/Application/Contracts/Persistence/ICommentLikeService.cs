using Application.DTOs;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface ICommentLikeService
{
    // Returns true if the comment was liked, false if it was unliked
    Task<CommentLikesDto> ToggleLikeAsync(Guid commentID, Guid userID, CancellationToken ct = default);
    Task<Result> LikeCommentAsync(Guid commentID, Guid userID, CancellationToken ct = default);
    Task<Result> UnlikeCommentAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default);
}