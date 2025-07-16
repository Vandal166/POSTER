using Application.Contracts;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _posts;
    private readonly IPostLikeRepository _postLike;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUnitOfWork _uow;

    public PostService(IPostRepository posts, IPostLikeRepository postLike, IValidator<CreatePostDto> createPostValidator, IUnitOfWork uow)
    {
        _posts = posts;
        _postLike = postLike;
        _createPostValidator = createPostValidator;
        _uow = uow;
    }

    public async Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken)
    {
        var validation = await _createPostValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var post = Domain.Entities.Post.Create(userID, dto.Content);
        if (post.IsFailed)
            return Result.Fail<Guid>(post.Errors.Select(e => e.Message));

        await _posts.AddAsync(post.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(post.Value.ID);
    }
    
    public async Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _posts.GetPostAsync(id, cancellationToken);
    }
    
    public IAsyncEnumerable<Post> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _posts.GetAllAsync(cancellationToken);
    }
    
    public async Task<Post?> DeletePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(id, cancellationToken);
        if (post == null)
            return null;

        await _posts.DeleteAsync(post, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return post;
    }

    public async Task<Result> LikePostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(postID, cancellationToken);
        if (post == null)
            return Result.Fail("Post not found");

        var alreadyLiked = await _postLike.IsPostLikedByUserAsync(postID, userID, cancellationToken);
        if(alreadyLiked)
            return Result.Ok();
        
        var likeResult = Domain.Entities.PostLike.Create(postID, userID);
        if (likeResult.IsFailed)
            return Result.Fail(likeResult.Errors.Select(e => e.Message));
        
        await _postLike.AddLikeAsync(likeResult.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
    
    public async Task<Result> UnlikePostAsync(Guid postID, Guid userID, CancellationToken ct = default)
    {
        var like = await _postLike.GetLikeAsync(postID, userID, ct);
        if(like is null)
            return Result.Fail("Like not found");
        
        await _postLike.RemoveLikeAsync(like, ct);
        await _uow.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}