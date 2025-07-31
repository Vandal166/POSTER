using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using FluentResults;

namespace Application.Services;

public class CommentLikeService : ICommentLikeService
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
        if(await _comments.GetCommentAsync(commentID, ct) is null)
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

    public async Task<Result> LikeCommentAsync(Guid commentID, Guid userID, CancellationToken ct = default)
    {
        if(await _comments.GetCommentAsync(commentID, ct) is null)
            return Result.Fail("Comment not found");
        
        var alreadyLiked = await _commentLikes.IsCommentLikedByUserAsync(commentID, userID, ct);
        if(alreadyLiked)
            return Result.Ok();
        
        var likeResult = Domain.Entities.CommentLike.Create(commentID, userID);
        if (likeResult.IsFailed)
            return Result.Fail(likeResult.Errors.Select(e => e.Message));
        
        await _commentLikes.AddLikeAsync(likeResult.Value, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok();
    }

    public async Task<Result> UnlikeCommentAsync(Guid commentID, Guid userID, CancellationToken ct = default)
    {
        var like = await _commentLikes.GetLikeAsync(commentID, userID, ct);
        if(like is null)
            return Result.Fail("Like not found");
        
        await _commentLikes.RemoveLikeAsync(like, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}