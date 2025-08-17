using Application.Contracts.Persistence;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class PostLikeRepository : IPostLikeRepository
{
    private readonly PosterDbContext _db;
    public PostLikeRepository(PosterDbContext db)
    {
        _db = db;
    }

    public async Task<PostLike?> GetLikeAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.PostLikes
            .AsNoTracking()
            .FirstOrDefaultAsync(pl => pl.PostID == postID && pl.UserID == userID, cancellationToken);
    }

    public async Task<bool> IsPostLikedByUserAsync(Guid postID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await _db.PostLikes
            .AsNoTracking()
            .AnyAsync(pl => pl.PostID == postID && pl.UserID == userID, cancellationToken);
    }

    public Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        _db.PostLikes.Add(like);
        return Task.CompletedTask;
    }

    public Task RemoveLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        _db.PostLikes.Remove(like);
        return Task.CompletedTask;
    }

    public async Task<List<PostLike>> GetLikesByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.PostLikes
            .AsNoTracking()
            .Where(pl => pl.PostID == postID)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetLikesCountByPostAsync(Guid postID, CancellationToken cancellationToken = default)
    {
        return await _db.PostLikes
            .AsNoTracking()
            .CountAsync(pl => pl.PostID == postID, cancellationToken);
    }
}