using Domain.Entities;

namespace Application.Contracts;

public interface IPostRepository
{
    Task<Post?> GetPostAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Post>> GetUserFeedAsync(Guid userId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Post> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);
}