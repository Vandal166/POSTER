using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostCommentService
{
    Task<Result> CreateCommentAsync(Guid postID, Guid userID, CreateCommentDto dto, CancellationToken cancellationToken = default);

    Task<Comment?> GetCommentAsync(Guid commentID, CancellationToken cancellationToken = default);
    Task<IPagedList<CommentDto>> GetAllCommentsAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteCommentAsync(Guid postID, Comment comment, CancellationToken cancellationToken = default);
}