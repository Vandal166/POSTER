using Application.Contracts.Persistence;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PostViewRepository : IPostViewRepository
{
    private readonly PosterDbContext _db;
    public PostViewRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasUserViewedPostAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.PostViews
            .AsNoTracking()
            .AnyAsync(pv => pv.PostID == postID && pv.UserID == userID, cancellationToken);
    }

    public Task AddViewAsync(PostView view, CancellationToken cancellationToken = default)
    {
        _db.PostViews.Add(view);
        return Task.CompletedTask;
    }

    public Task RemoveViewAsync(PostView view, CancellationToken cancellationToken = default)
    {
        _db.PostViews.Remove(view);
        return Task.CompletedTask;
    }

    public async Task<List<PostView>> GetViewsByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.PostViews
            .AsNoTracking()
            .Where(pv => pv.PostID == postID)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetViewsCountByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.PostViews
            .AsNoTracking()
            .CountAsync(pv => pv.PostID == postID, cancellationToken);
    }
}