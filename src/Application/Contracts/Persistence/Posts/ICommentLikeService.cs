using Application.DTOs;

namespace Application.Contracts.Persistence;

public interface ICommentLikeService
{
    // Toggles an like - Returns true if the comment was liked, false if it was unliked
    Task<CommentLikesDto> ToggleLikeAsync(Guid commentID, Guid userID, CancellationToken ct = default);
}