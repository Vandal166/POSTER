using Application.DTOs;
using Domain.Entities;
using FluentResults;

namespace Application.Contracts.Persistence;

public interface IPostCommentService
{
    Task<Result> CreateCommentAsync(Guid postID, Guid userID, CreateCommentDto dto, CancellationToken ct = default);
    Task<Result> CreateCommentOnCommentAsync(Guid postID, Guid parentCommentID, Guid userID, CreateCommentDto dto, CancellationToken ct = default);

    Task<Comment?> GetCommentAsync(Guid commentID, CancellationToken cancellationToken = default);
    
    Task<Result<bool>> DeleteCommentAsync(Guid id, Guid currentUserID, CancellationToken ct = default);
}