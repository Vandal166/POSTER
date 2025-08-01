namespace Application.Contracts.Persistence;

public interface IPostViewService
{
    Task<bool> HasUserViewedPostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    Task<bool> AddViewAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    Task RemoveViewAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);
    
    Task<int> GetViewsCountByPostAsync(Guid postID, CancellationToken cancellationToken = default);
}