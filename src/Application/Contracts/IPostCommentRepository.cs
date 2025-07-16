using Domain.Entities;

namespace Application.Contracts;

public interface IPostCommentRepository
{
    Task<Comment?> GetCommentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetCommentsByPostAsync(Guid postID, CancellationToken cancellationToken = default);
    
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
}