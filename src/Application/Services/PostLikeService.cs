using Application.Contracts;
using Application.Contracts.Persistence;
using FluentResults;

namespace Application.Services;

public class PostLikeService : IPostLikeService
{
    private readonly IPostLikeRepository _postLikes;
    private readonly IPostRepository _posts;
    private readonly IUnitOfWork _uow;

    public PostLikeService(IPostLikeRepository postLikeRepository, IPostRepository posts ,IUnitOfWork uow)
    {
        _postLikes = postLikeRepository;
        _posts = posts;
        _uow = uow;
    }

    public async Task<Result> LikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        if(await _posts.ExistsAsync(postID, cancellationToken) == false)
            return Result.Fail("Post not found");
        
        var alreadyLiked = await _postLikes.IsPostLikedByUserAsync(postID, userID, cancellationToken);
        if(alreadyLiked)
            return Result.Ok();
        
        var likeResult = Domain.Entities.PostLike.Create(postID, userID);
        if (likeResult.IsFailed)
            return Result.Fail(likeResult.Errors.Select(e => e.Message));
        
        await _postLikes.AddLikeAsync(likeResult.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
    
    public async Task<Result> UnlikePostAsync(Guid postID, Guid userID, CancellationToken ct = default)
    {
        var like = await _postLikes.GetLikeAsync(postID, userID, ct);
        if(like is null)
            return Result.Fail("Like not found");
        
        await _postLikes.RemoveLikeAsync(like, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}