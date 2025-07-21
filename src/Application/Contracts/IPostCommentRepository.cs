using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts;

public interface IPostCommentRepository
{
    Task<Comment?> GetCommentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IPagedList<CommentDto>> GetCommentsByPostAsync(Guid postID, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
}