using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;

namespace Application.Services;

internal sealed class PostLikeService : IPostLikeService
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
    
    public async Task<PostLikesDto> ToggleLikeAsync(Guid postID, Guid userID, CancellationToken ct = default)
    {
        if (await _posts.ExistsAsync(postID, ct) == false)
            throw new KeyNotFoundException("Post not found");
        
        var alreadyLiked = await _postLikes.IsPostLikedByUserAsync(postID, userID, ct);
        if (alreadyLiked)
        {
            var like = await _postLikes.GetLikeAsync(postID, userID, ct);
            if (like is null)
                throw new InvalidOperationException("Like not found");
            
            await _postLikes.RemoveLikeAsync(like, ct);
        }
        else
        {
            var likeResult = Domain.Entities.PostLike.Create(postID, userID);
            if (likeResult.IsFailed)
                throw new InvalidOperationException(string.Join(", ", likeResult.Errors.Select(e => e.Message)));
            
            await _postLikes.AddLikeAsync(likeResult.Value, ct);
        }
        
        await _uow.SaveChangesAsync(ct);
        var likesCount = await _postLikes.GetLikesByPostAsync(postID, ct);
        
        return new PostLikesDto(postID, likesCount.Count, !alreadyLiked); // returns the updated likes count and whether it was liked or unliked
    }
}