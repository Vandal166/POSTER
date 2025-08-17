using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IPostCommentRepository
{
    Task<bool> ExistsAsync(Guid commentID, CancellationToken cancellationToken = default);
    
    Task<Comment?> GetCommentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CommentDto?> GetCommentDtoAsync(Guid id, CancellationToken cancellationToken = default);
    
    // getting nested comments by commentID
    Task<IPagedList<CommentDto>> GetCommentsByCommentAsync(Guid commentID, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IPagedList<CommentDto>> GetCommentsByPostAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<int> GetCommentsCountByPostAsync(Guid postID, CancellationToken ct = default);
    Task<int> GetCommentsCountByCommentAsync(Guid commentID, CancellationToken ct = default);
    
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
}