using Application.DTOs;
using Domain.Entities;

namespace Application.Contracts;

public interface IPostRepository
{
    Task<bool> ExistsAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IPagedList<PostDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}

public interface IPagedList<T>
{
    List<T> Items { get; }
    int TotalCount { get; }
    int Page { get; }
    int PageSize { get; }
}