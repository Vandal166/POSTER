using Application.Contracts;
using Application.Contracts.Persistence;
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

    public async Task<Result> CreateCommentAsync(Guid postID, Guid userID, CreateCommentDto dto, CancellationToken ct = default)
    {
        if(await _posts.ExistsAsync(postID, ct) == false)
            return Result.Fail("Post not found");

        var validation = await _createCommentValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var comment = Comment.Create(postID, userID, dto.Content);
        if (comment.IsFailed)
            return Result.Fail(comment.Errors.Select(e => e.Message));

        await _postComments.AddAsync(comment.Value, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
   
    public async Task<Result> CreateCommentOnCommentAsync(Guid postID, Guid parentCommentID, Guid userID, CreateCommentDto dto, CancellationToken ct = default)
    {
        if(await _posts.ExistsAsync(postID, ct) == false)
            return Result.Fail("Post not found");
        
        if(await _postComments.ExistsAsync(parentCommentID, ct) == false)
            return Result.Fail("Comment not found");

        var validation = await _createCommentValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var comment = Comment.Create(postID, userID, dto.Content, parentCommentID);
        if (comment.IsFailed)
            return Result.Fail(comment.Errors.Select(e => e.Message));

        await _postComments.AddAsync(comment.Value, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
    
    public async Task<Comment?> GetCommentAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _postComments.GetCommentAsync(commentID, cancellationToken);
    }
    
    public async Task<CommentDto?> GetCommentDtoAsync(Guid commentID, CancellationToken cancellationToken = default)
    {
        return await _postComments.GetCommentDtoAsync(commentID, cancellationToken);
    }

    public async Task<IPagedList<CommentDto>> GetAllCommentsAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (await _posts.ExistsAsync(postID, cancellationToken) == false)
            throw new KeyNotFoundException("Post not found");

        return await _postComments.GetCommentsByPostAsync(postID, page, pageSize, cancellationToken);
    }

    public async Task<Result<bool>> DeleteCommentAsync(Guid id, Guid currentUserID, CancellationToken ct = default)
    {
        var comment = await _postComments.GetCommentAsync(id, ct);
        
        if (comment is null)
            return Result.Fail<bool>("Comment not found");

        if (comment.AuthorID != currentUserID)
            return Result.Fail<bool>("You can only delete your own comments");
        
        await _postComments.DeleteAsync(comment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Ok(true);
    }

    public async Task<Result> DeleteCommentAsync(Guid postID, Comment comment, CancellationToken cancellationToken = default)
    {
        var post = await _posts.GetPostAsync(postID, cancellationToken);
        
        if (post is null)
            return Result.Fail("Post not found");
        
        if(post.Id != comment.PostID)
            return Result.Fail("Comment does not belong to this post");

        await _postComments.DeleteAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}