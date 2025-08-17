using Application.Contracts;
using Application.Contracts.Persistence;

namespace Application.Services;

internal sealed class PostViewService : IPostViewService
{
    private readonly IPostViewRepository _postViews;
    private readonly IPostRepository _posts;
    private readonly IUnitOfWork _uow;

    public PostViewService(IPostViewRepository postViewRepository, IPostRepository posts, IUnitOfWork uow)
    {
        _postViews = postViewRepository;
        _posts = posts;
        _uow = uow;
    }

    public async Task<bool> HasUserViewedPostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _postViews.HasUserViewedPostAsync(postID, userID, cancellationToken);
    }

    public async Task<bool> AddViewAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        if (await _posts.ExistsAsync(postID, cancellationToken) == false)
            return false;

        var alreadyViewed = await HasUserViewedPostAsync(postID, userID, cancellationToken);
        if (alreadyViewed)
            return false; // No need to add a view if it already exists

        var viewResult = Domain.Entities.PostView.Create(postID, userID);
        if (viewResult.IsFailed)
            return false;

        await _postViews.AddViewAsync(viewResult.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return true; // Successfully added the view
    }

    public async Task<int> GetViewsCountByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _postViews.GetViewsCountByPostAsync(postID, cancellationToken);
    }
}