using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts;

public interface IPostCommentService
{
    Task<Result> CreateCommentAsync(Guid postID, Guid userID, CreateCommentDto dto, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetAllCommentsAsync(Guid postID, CancellationToken cancellationToken = default);
    Task<Result> DeleteCommentAsync(Guid commentID, Guid userID, CancellationToken cancellationToken = default);
}