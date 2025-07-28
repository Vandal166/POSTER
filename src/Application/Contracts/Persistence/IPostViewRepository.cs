using Domain.Entities;

namespace Application.Contracts.Persistence;

public interface IPostViewRepository
{
    Task<bool> HasUserViewedPostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    
    Task AddViewAsync(PostView view, CancellationToken cancellationToken = default);
    Task RemoveViewAsync(PostView view, CancellationToken cancellationToken = default);
    
    Task<List<PostView>> GetViewsByPostAsync(Guid postID, CancellationToken cancellationToken = default);
    Task<int> GetViewsCountByPostAsync(Guid postID, CancellationToken cancellationToken = default);
}