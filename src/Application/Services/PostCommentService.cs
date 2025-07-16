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

    public async Task<List<Comment>> GetAllCommentsAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        if (await _posts.ExistsAsync(postID, cancellationToken) == false)
            return new List<Comment>();
        
        var comments = await _postComments.GetCommentsByPostAsync(postID, cancellationToken);
        if (comments.Count == 0)
            return new List<Comment>();

        return comments;
    }

    public async Task<Result> DeleteCommentAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default)
    {
        var comment = await _postComments.GetCommentAsync(commentID, cancellationToken);
        if (comment is null)
            return Result.Fail("Comment not found");

        await _postComments.DeleteAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}