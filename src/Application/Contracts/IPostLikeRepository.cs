using Domain.Entities;

namespace Application.Contracts;

public interface IPostLikeRepository
{
    Task<PostLike?> GetLikeAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    Task<bool> IsPostLikedByUserAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    
    Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default);
    Task RemoveLikeAsync(PostLike like, CancellationToken cancellationToken = default);
    
    Task<List<PostLike>> GetLikesByPostAsync(Guid postID, CancellationToken cancellationToken = default);
}