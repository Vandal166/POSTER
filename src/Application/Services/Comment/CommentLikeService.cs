using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;

namespace Application.Services;

internal sealed class CommentLikeService : ICommentLikeService
{
    private readonly ICommentLikeRepository _commentLikes;
    private readonly IPostCommentRepository _comments;
    private readonly IUnitOfWork _uow;
    
    public CommentLikeService(ICommentLikeRepository commentLikeRepository, IPostCommentRepository comments, IUnitOfWork uow)
    {
        _commentLikes = commentLikeRepository;
        _comments = comments;
        _uow = uow;
    }

    public async Task<CommentLikesDto> ToggleLikeAsync(Guid commentID, Guid userID, CancellationToken ct = default)
    {
        if(await _comments.GetCommentDtoAsync(commentID, ct) is null)
            throw new KeyNotFoundException("Comment not found");
        
        var alreadyLiked = await _commentLikes.IsCommentLikedByUserAsync(commentID, userID, ct);
        if (alreadyLiked)
        {
            var like = await _commentLikes.GetLikeAsync(commentID, userID, ct);
            if (like is null)
                throw new InvalidOperationException("Like not found");
            
            await _commentLikes.RemoveLikeAsync(like, ct);
        }
        else
        {
            var likeResult = Domain.Entities.CommentLike.Create(commentID, userID);
            if (likeResult.IsFailed)
                throw new InvalidOperationException(string.Join(", ", likeResult.Errors.Select(e => e.Message)));
            
            await _commentLikes.AddLikeAsync(likeResult.Value, ct);
        }
        
        await _uow.SaveChangesAsync(ct);
        var likesCount = await _commentLikes.GetLikesByCommentAsync(commentID, ct);
        
        return new CommentLikesDto(commentID, likesCount.Count, !alreadyLiked); // returns the updated likes count and whether it was liked or unliked
    }
}