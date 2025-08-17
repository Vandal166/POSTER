using Application.DTOs;

namespace Application.Contracts.Persistence;

public interface IPostLikeService
{
    // Toggles an like -  Returns true if the post was liked, false if it was unliked
    Task<PostLikesDto> ToggleLikeAsync(Guid postID, Guid userID, CancellationToken ct = default);
}