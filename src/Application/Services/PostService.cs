using Application.Contracts;
using Application.Contracts.Persistence;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _posts;
    private readonly IPostImageRepository _postImages;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUnitOfWork _uow;

    public PostService(IPostRepository posts, IPostImageRepository postImages, IValidator<CreatePostDto> createPostValidator, IUnitOfWork uow)
    {
        _posts = posts;
        _postImages = postImages;
        _createPostValidator = createPostValidator;
        _uow = uow;
    }

    public async Task<Result<Guid>> CreatePostAsync(CreatePostDto dto, Guid userID, CancellationToken cancellationToken)
    {
        var validation = await _createPostValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail<Guid>(validation.Errors.Select(e => e.ErrorMessage));
        
        var post = Post.Create(userID, dto.Content, dto.VideoFileID);
        var postImages = new List<PostImage>();
        foreach (var imageIDs in dto.ImageFileIDs ?? Enumerable.Empty<Guid>())
        {
            var postImageResult = PostImage.Create(post.Value.ID, imageIDs);
            if (postImageResult.IsFailed)
                return Result.Fail<Guid>(postImageResult.Errors.Select(e => e.Message));
            postImages.Add(postImageResult.Value);
        }
        if (post.IsFailed)
            return Result.Fail<Guid>(post.Errors.Select(e => e.Message));

        await _posts.AddAsync(post.Value, cancellationToken);
        await _postImages.AddRangeAsync(postImages, cancellationToken);
        
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(post.Value.ID);
    }
    
    public async Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        //return await _posts.GetPostAsync(id, cancellationToken);
        return null;
    }
    
    public async Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        //return await _posts.GetAllAsync(page, pageSize, cancellationToken);
        return null;
    }
    
    public async Task<Post?> DeletePostAsync(Guid id, CancellationToken cancellationToken = default)
    {
        /*var post = await _posts.GetPostAsync(id, cancellationToken);
        if (post is null)
            return null;
        
        //TODO delete comments and likes before via injecting their repos
        await _posts.DeleteAsync(post, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return post;*/
        return null;
    }
}