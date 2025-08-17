namespace Application.Contracts.Persistence;

public interface IPostViewService
{
    Task<bool> AddViewAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default);

    Task<int> GetViewsCountByPostAsync(Guid postID, CancellationToken cancellationToken = default);
}