using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IPostRepository
{
    Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default);
   // Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostAsync(Guid postID, CancellationToken ct = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}