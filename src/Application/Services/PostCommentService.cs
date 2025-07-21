using Application.Contracts;
using Application.DTOs;
using Domain.Entities;
using FluentResults;
using FluentValidation;

namespace Application.Services;

public class PostCommentService : IPostCommentService
{
    private readonly IPostCommentRepository _postComments;
    private readonly IPostRepository _posts;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IUnitOfWork _uow;

    public PostCommentService(IPostCommentRepository postComments, IPostRepository posts, IValidator<CreateCommentDto> validator,
        IUnitOfWork uow)
    {
        _postComments = postComments;
        _posts = posts;
        _createCommentValidator = validator;
        _uow = uow;
    }

    public async Task<Result> CreateCommentAsync(Guid postID, Guid userID, CreateCommentDto dto,
        CancellationToken cancellationToken = default)
    {
        if(await _posts.ExistsAsync(postID, cancellationToken) == false)
            return Result.Fail("Post not found");

        var validation = await _createCommentValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var comment = Comment.Create(postID, userID, dto.Content);
        if (comment.IsFailed)
            return Result.Fail(comment.Errors.Select(e => e.Message));

        await _postComments.AddAsync(comment.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    
    public async Task<Comment?> GetCommentAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _postComments.GetCommentAsync(commentID, cancellationToken);
    }

    public async Task<IPagedList<CommentDto>> GetAllCommentsAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (await _posts.ExistsAsync(postID, cancellationToken) == false)
            throw new KeyNotFoundException("Post not found");

        return await _postComments.GetCommentsByPostAsync(postID, page, pageSize, cancellationToken);
    }

    public async Task<Result> DeleteCommentAsync(Guid postID, Comment comment, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(postID, cancellationToken);
        
        if (post is null)
            return Result.Fail("Post not found");
        
        if(post.ID != comment.PostID)
            return Result.Fail("Comment does not belong to this post");

        await _postComments.DeleteAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}