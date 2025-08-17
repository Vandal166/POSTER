using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface ICommentLikeRepository
{
    Task<CommentLike?> GetLikeAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default);
    Task<bool> IsCommentLikedByUserAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default);
    
    Task AddLikeAsync(CommentLike like, CancellationToken cancellationToken = default);
    Task RemoveLikeAsync(CommentLike like, CancellationToken cancellationToken = default);
    
    Task<List<CommentLike>> GetLikesByCommentAsync(Guid commentID, CancellationToken cancellationToken = default);
    Task<int> GetLikesCountByCommentAsync(Guid postID, CancellationToken cancellationToken = default);
}